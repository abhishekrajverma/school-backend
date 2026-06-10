using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Students.Application;
using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Students;

public sealed class BulkPromoteStudentsCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch,
    ICurrentUserContext user)
    : IRequestHandler<BulkPromoteStudentsCommand, Result<PromotionResultDto>>
{
    public async Task<Result<PromotionResultDto>> Handle(BulkPromoteStudentsCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue || !branch.BranchId.HasValue)
        {
            return Result<PromotionResultDto>.Failure(Error.Forbidden("Tenant and branch are required."));
        }

        var body = request.Request;
        var rules = body.Rules.ToDictionary(
            r => r.FromClass.Trim(),
            r => r.ToClass.Trim(),
            StringComparer.OrdinalIgnoreCase);

        var enrollments = await db.StudentEnrollments
            .Include(e => e.Student)
            .Where(e => e.TenantId == tenant.TenantId
                        && e.BranchId == branch.BranchId
                        && e.AcademicYearId == body.FromAcademicYearId
                        && e.EnrollmentStatus == EnrollmentStatuses.Enrolled
                        && !e.IsDeleted
                        && !e.Student.IsDeleted)
            .ToListAsync(ct);

        var batch = new PromotionBatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            BranchId = branch.BranchId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FromAcademicYearId = body.FromAcademicYearId,
            ToAcademicYearId = body.ToAcademicYearId,
            Status = PromotionBatchStatuses.Completed,
            TotalStudents = enrollments.Count,
            ExecutedByUserId = user.UserId,
            ExecutedAt = DateTime.UtcNow,
        };

        var promoted = 0;
        var skipped = 0;

        foreach (var current in enrollments)
        {
            if (!LifecycleStatuses.IsPromotable(current.Student.LifecycleStatus))
            {
                skipped++;
                batch.Items.Add(new PromotionBatchItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.TenantId.Value,
                    PromotionBatchId = batch.Id,
                    StudentId = current.StudentId,
                    FromEnrollmentId = current.Id,
                    Outcome = PromotionOutcomes.SkippedInactive,
                    SkipReason = "Student is not active",
                });
                continue;
            }

            if (!rules.TryGetValue(current.ClassName, out var targetClass))
            {
                skipped++;
                batch.Items.Add(new PromotionBatchItem
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenant.TenantId.Value,
                    PromotionBatchId = batch.Id,
                    StudentId = current.StudentId,
                    FromEnrollmentId = current.Id,
                    Outcome = PromotionOutcomes.SkippedNoTargetClass,
                    SkipReason = $"No promotion rule for class '{current.ClassName}'",
                });
                continue;
            }

            current.EnrollmentStatus = EnrollmentStatuses.Promoted;
            current.ClosedAt = DateTime.UtcNow;

            var newEnrollment = new StudentEnrollment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                BranchId = branch.BranchId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                StudentId = current.StudentId,
                AcademicYearId = body.ToAcademicYearId,
                ClassName = targetClass,
                Section = current.Section,
                RollNo = current.RollNo,
                EnrollmentStatus = EnrollmentStatuses.Enrolled,
                PromotedFromEnrollmentId = current.Id,
                EnrolledAt = DateTime.UtcNow,
            };

            db.StudentEnrollments.Add(newEnrollment);
            promoted++;
            batch.Items.Add(new PromotionBatchItem
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                PromotionBatchId = batch.Id,
                StudentId = current.StudentId,
                FromEnrollmentId = current.Id,
                ToEnrollmentId = newEnrollment.Id,
                Outcome = PromotionOutcomes.Promoted,
            });
        }

        batch.PromotedCount = promoted;
        batch.SkippedCount = skipped;
        db.PromotionBatches.Add(batch);
        await db.SaveChangesAsync(ct);

        return Result<PromotionResultDto>.Success(new PromotionResultDto(
            batch.ExternalId,
            batch.Status,
            batch.TotalStudents,
            batch.PromotedCount,
            batch.SkippedCount));
    }
}

public sealed class RollbackPromotionBatchCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<RollbackPromotionBatchCommand, Result<PromotionResultDto>>
{
    public async Task<Result<PromotionResultDto>> Handle(RollbackPromotionBatchCommand request, CancellationToken ct)
    {
        var batch = await db.PromotionBatches
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.ExternalId == request.BatchExternalId && !b.IsDeleted, ct);

        if (batch is null)
        {
            return Result<PromotionResultDto>.Failure(Error.NotFound("Promotion batch not found."));
        }

        if (batch.Status == PromotionBatchStatuses.RolledBack)
        {
            return Result<PromotionResultDto>.Success(new PromotionResultDto(
                batch.ExternalId, batch.Status, batch.TotalStudents, batch.PromotedCount, batch.SkippedCount));
        }

        foreach (var item in batch.Items.Where(i => i.ToEnrollmentId.HasValue))
        {
            var newEnrollment = await db.StudentEnrollments
                .FirstOrDefaultAsync(e => e.Id == item.ToEnrollmentId, ct);
            if (newEnrollment is not null)
            {
                newEnrollment.IsDeleted = true;
                newEnrollment.EnrollmentStatus = EnrollmentStatuses.Withdrawn;
                newEnrollment.ClosedAt = DateTime.UtcNow;
            }

            var oldEnrollment = await db.StudentEnrollments
                .FirstOrDefaultAsync(e => e.Id == item.FromEnrollmentId, ct);
            if (oldEnrollment is not null)
            {
                oldEnrollment.EnrollmentStatus = EnrollmentStatuses.Enrolled;
                oldEnrollment.ClosedAt = null;
            }
        }

        batch.Status = PromotionBatchStatuses.RolledBack;
        batch.RolledBackAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Result<PromotionResultDto>.Success(new PromotionResultDto(
            batch.ExternalId, batch.Status, batch.TotalStudents, batch.PromotedCount, batch.SkippedCount));
    }
}
