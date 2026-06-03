using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Exams.Application;
using EduSync.Modules.Exams.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Exams;

internal static class ExamMapping
{
    public static ExamDto ToDto(Exam e) => new(
        e.ExternalId, e.ExamName, e.ExamType, e.Subject, e.ClassName,
        e.Date.ToString("yyyy-MM-dd"), e.StartTime, e.DurationMinutes,
        e.TotalMarks, e.PassingMarks, e.Room, e.Status, e.StudentsCount);
}

public sealed class ListExamsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListExamsQuery, Result<PaginatedList<ExamDto>>>
{
    public async Task<Result<PaginatedList<ExamDto>>> Handle(ListExamsQuery request, CancellationToken ct)
    {
        var query = db.Exams.AsNoTracking().Where(e => !e.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.ClassName)) query = query.Where(e => e.ClassName == request.ClassName);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(e => e.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(e => e.ExamName.ToLower().Contains(term) || e.Subject.ToLower().Contains(term));
        }

        query = query.OrderBy(e => e.Date);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(ExamMapping.ToDto).ToList();
        return Result<PaginatedList<ExamDto>>.Success(
            PaginatedList<ExamDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetExamByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetExamByIdQuery, Result<ExamDto>>
{
    public async Task<Result<ExamDto>> Handle(GetExamByIdQuery request, CancellationToken ct)
    {
        var e = await db.Exams.AsNoTracking().FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return e is null ? Result<ExamDto>.Failure(Error.NotFound("Exam not found."))
            : Result<ExamDto>.Success(ExamMapping.ToDto(e));
    }
}

public sealed class CreateExamCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateExamCommand, Result<ExamDto>>
{
    public async Task<Result<ExamDto>> Handle(CreateExamCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<ExamDto>.Failure(Error.Forbidden("Tenant required."));
        var body = request.Request;
        if (!DateOnly.TryParse(body.Date, out var date)) return Result<ExamDto>.Failure(Error.Validation("Invalid date."));

        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            ExamName = body.ExamName,
            ExamType = body.ExamType,
            Subject = body.Subject,
            ClassName = body.Class,
            Date = date,
            StartTime = body.StartTime,
            DurationMinutes = body.Duration,
            TotalMarks = body.TotalMarks,
            PassingMarks = body.PassingMarks,
            Room = body.Room,
            Status = body.Status,
            StudentsCount = body.StudentsCount,
        };
        db.Exams.Add(exam);
        await db.SaveChangesAsync(ct);
        return Result<ExamDto>.Success(ExamMapping.ToDto(exam));
    }
}

public sealed class UpdateExamCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateExamCommand, Result<ExamDto>>
{
    public async Task<Result<ExamDto>> Handle(UpdateExamCommand request, CancellationToken ct)
    {
        var exam = await db.Exams.FirstOrDefaultAsync(e => e.ExternalId == request.ExternalId && !e.IsDeleted, ct);
        if (exam is null) return Result<ExamDto>.Failure(Error.NotFound("Exam not found."));
        var body = request.Request;
        if (body.ExamName is not null) exam.ExamName = body.ExamName;
        if (body.ExamType is not null) exam.ExamType = body.ExamType;
        if (body.Subject is not null) exam.Subject = body.Subject;
        if (body.Class is not null) exam.ClassName = body.Class;
        if (body.Date is not null && DateOnly.TryParse(body.Date, out var d)) exam.Date = d;
        if (body.StartTime is not null) exam.StartTime = body.StartTime;
        if (body.Duration is not null) exam.DurationMinutes = body.Duration.Value;
        if (body.TotalMarks is not null) exam.TotalMarks = body.TotalMarks.Value;
        if (body.PassingMarks is not null) exam.PassingMarks = body.PassingMarks.Value;
        if (body.Room is not null) exam.Room = body.Room;
        if (body.Status is not null) exam.Status = body.Status;
        if (body.StudentsCount is not null) exam.StudentsCount = body.StudentsCount.Value;
        await db.SaveChangesAsync(ct);
        return Result<ExamDto>.Success(ExamMapping.ToDto(exam));
    }
}

public sealed class DeleteExamCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteExamCommand, Result>
{
    public async Task<Result> Handle(DeleteExamCommand request, CancellationToken ct)
    {
        var exam = await db.Exams.FirstOrDefaultAsync(e => e.ExternalId == request.ExternalId && !e.IsDeleted, ct);
        if (exam is null) return Result.Failure(Error.NotFound("Exam not found."));
        exam.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
