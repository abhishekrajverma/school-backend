using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Staff.Application;
using EduSync.Modules.Staff.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Staff;

internal static class TeacherAssignmentMapping
{
    public static TeacherAssignmentDto ToDto(TeacherAssignment a, string teacherExternalId) => new(
        a.ExternalId,
        teacherExternalId,
        a.AcademicYearId.ToString(),
        a.ClassName,
        a.SubjectExternalId,
        a.AssignmentType,
        a.IsActive);
}

public sealed class ListTeacherAssignmentsQueryHandler(
    EduSyncDbContext db,
    IBranchContext branch,
    IAcademicYearContext academicYear)
    : IRequestHandler<ListTeacherAssignmentsQuery, Result<IReadOnlyList<TeacherAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<TeacherAssignmentDto>>> Handle(
        ListTeacherAssignmentsQuery request,
        CancellationToken ct)
    {
        var query = db.TeacherAssignments.AsNoTracking().Where(a => !a.IsDeleted && a.IsActive);

        if (!string.IsNullOrWhiteSpace(request.TeacherExternalId))
        {
            var teacher = await db.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(t => t.ExternalId == request.TeacherExternalId && !t.IsDeleted, ct);
            if (teacher is null)
            {
                return Result<IReadOnlyList<TeacherAssignmentDto>>.Failure(Error.NotFound("Teacher not found."));
            }

            query = query.Where(a => a.TeacherId == teacher.Id);
        }

        var yearId = request.AcademicYearId ?? academicYear.AcademicYearId;
        if (yearId.HasValue)
        {
            query = query.Where(a => a.AcademicYearId == yearId);
        }

        if (branch.IsResolved)
        {
            query = query.Where(a => a.BranchId == branch.BranchId);
        }

        var items = await query
            .Join(db.Teachers.AsNoTracking(), a => a.TeacherId, t => t.Id, (a, t) => TeacherAssignmentMapping.ToDto(a, t.ExternalId))
            .ToListAsync(ct);

        return Result<IReadOnlyList<TeacherAssignmentDto>>.Success(items);
    }
}

public sealed class CreateTeacherAssignmentCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch)
    : IRequestHandler<CreateTeacherAssignmentCommand, Result<TeacherAssignmentDto>>
{
    public async Task<Result<TeacherAssignmentDto>> Handle(CreateTeacherAssignmentCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue || !branch.BranchId.HasValue)
        {
            return Result<TeacherAssignmentDto>.Failure(Error.Forbidden("Tenant and branch are required."));
        }

        var teacher = await db.Teachers.FirstOrDefaultAsync(
            t => t.ExternalId == request.Request.TeacherExternalId && !t.IsDeleted,
            ct);
        if (teacher is null)
        {
            return Result<TeacherAssignmentDto>.Failure(Error.NotFound("Teacher not found."));
        }

        var assignment = new TeacherAssignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            BranchId = branch.BranchId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            TeacherId = teacher.Id,
            AcademicYearId = request.Request.AcademicYearId,
            ClassName = request.Request.ClassName,
            SubjectExternalId = request.Request.SubjectExternalId,
            AssignmentType = request.Request.AssignmentType,
            IsActive = true,
        };

        db.TeacherAssignments.Add(assignment);
        await db.SaveChangesAsync(ct);
        return Result<TeacherAssignmentDto>.Success(TeacherAssignmentMapping.ToDto(assignment, teacher.ExternalId));
    }
}

public sealed class DeactivateTeacherAssignmentCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeactivateTeacherAssignmentCommand, Result>
{
    public async Task<Result> Handle(DeactivateTeacherAssignmentCommand request, CancellationToken ct)
    {
        var assignment = await db.TeacherAssignments
            .FirstOrDefaultAsync(a => a.ExternalId == request.ExternalId && !a.IsDeleted, ct);
        if (assignment is null)
        {
            return Result.Failure(Error.NotFound("Teacher assignment not found."));
        }

        assignment.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
