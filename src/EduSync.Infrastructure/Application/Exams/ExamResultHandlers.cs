using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Exams.Application;
using EduSync.Modules.Exams.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Exams;

internal static class ExamResultMapping
{
    public static ExamResultDto ToDto(ExamResult r) => new(
        r.ExternalId,
        r.ExamExternalId,
        r.StudentExternalId,
        r.MarksObtained,
        r.TotalMarks,
        r.Grade,
        r.Status,
        r.Remarks);
}

public sealed class ListExamResultsQueryHandler(
    EduSyncDbContext db,
    IAcademicYearContext academicYear)
    : IRequestHandler<ListExamResultsQuery, Result<IReadOnlyList<ExamResultDto>>>
{
    public async Task<Result<IReadOnlyList<ExamResultDto>>> Handle(ListExamResultsQuery request, CancellationToken ct)
    {
        var query = db.ExamResults.AsNoTracking().Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.ExamExternalId))
        {
            query = query.Where(r => r.ExamExternalId == request.ExamExternalId);
        }

        if (!string.IsNullOrWhiteSpace(request.StudentExternalId))
        {
            query = query.Where(r => r.StudentExternalId == request.StudentExternalId);
        }

        var yearId = request.AcademicYearId ?? academicYear.AcademicYearId;
        if (yearId.HasValue)
        {
            query = query.Where(r => r.AcademicYearId == yearId);
        }

        var items = await query.OrderByDescending(r => r.CreatedAt).Select(r => ExamResultMapping.ToDto(r)).ToListAsync(ct);
        return Result<IReadOnlyList<ExamResultDto>>.Success(items);
    }
}

public sealed class RecordExamResultCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IAcademicYearContext academicYear,
    IBranchContext branch)
    : IRequestHandler<RecordExamResultCommand, Result<ExamResultDto>>
{
    public async Task<Result<ExamResultDto>> Handle(RecordExamResultCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue || !academicYear.AcademicYearId.HasValue)
        {
            return Result<ExamResultDto>.Failure(Error.Forbidden("Tenant and academic year are required."));
        }

        var body = request.Request;
        var exam = await db.Exams.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExternalId == body.ExamExternalId && !e.IsDeleted, ct);
        if (exam is null)
        {
            return Result<ExamResultDto>.Failure(Error.NotFound("Exam not found."));
        }

        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == body.StudentExternalId && !s.IsDeleted, ct);
        if (student is null)
        {
            return Result<ExamResultDto>.Failure(Error.NotFound("Student not found."));
        }

        var existing = await db.ExamResults
            .FirstOrDefaultAsync(
                r => r.ExamExternalId == body.ExamExternalId
                     && r.StudentExternalId == body.StudentExternalId
                     && !r.IsDeleted,
                ct);

        if (existing is not null)
        {
            existing.MarksObtained = body.MarksObtained;
            existing.TotalMarks = body.TotalMarks;
            existing.Grade = body.Grade;
            existing.Remarks = body.Remarks;
            existing.Status = ExamResultStatuses.Published;
        }
        else
        {
            existing = new ExamResult
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                BranchId = branch.BranchId,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                AcademicYearId = academicYear.AcademicYearId.Value,
                ExamExternalId = body.ExamExternalId,
                StudentExternalId = body.StudentExternalId,
                MarksObtained = body.MarksObtained,
                TotalMarks = body.TotalMarks,
                Grade = body.Grade,
                Remarks = body.Remarks,
                Status = ExamResultStatuses.Published,
            };
            db.ExamResults.Add(existing);
        }

        await db.SaveChangesAsync(ct);
        return Result<ExamResultDto>.Success(ExamResultMapping.ToDto(existing));
    }
}
