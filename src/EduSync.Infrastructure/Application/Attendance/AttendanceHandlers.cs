using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Attendance.Application;
using EduSync.Modules.Attendance.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Attendance;

internal static class AttendanceMapping
{
    public static AttendanceRecordDto ToDto(AttendanceRecord r) => new(
        r.ExternalId, r.EntityType, r.EntityExternalId, r.EntityName, r.ClassName,
        r.Date.ToString("yyyy-MM-dd"), r.Status, r.CheckIn, r.CheckOut, r.Remarks);
}

public sealed class ListAttendanceQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListAttendanceQuery, Result<PaginatedList<AttendanceRecordDto>>>
{
    public async Task<Result<PaginatedList<AttendanceRecordDto>>> Handle(ListAttendanceQuery request, CancellationToken ct)
    {
        var query = db.AttendanceRecords.AsNoTracking().Where(a => !a.IsDeleted);
        if (DateOnly.TryParse(request.Date, out var date)) query = query.Where(a => a.Date == date);
        if (!string.IsNullOrWhiteSpace(request.EntityType)) query = query.Where(a => a.EntityType == request.EntityType);
        if (!string.IsNullOrWhiteSpace(request.ClassName)) query = query.Where(a => a.ClassName == request.ClassName);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(a => a.EntityName.ToLower().Contains(term) || a.EntityExternalId.Contains(term));
        }

        query = query.OrderByDescending(a => a.Date).ThenBy(a => a.EntityName);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(AttendanceMapping.ToDto).ToList();
        return Result<PaginatedList<AttendanceRecordDto>>.Success(
            PaginatedList<AttendanceRecordDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetAttendanceByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetAttendanceByIdQuery, Result<AttendanceRecordDto>>
{
    public async Task<Result<AttendanceRecordDto>> Handle(GetAttendanceByIdQuery request, CancellationToken ct)
    {
        var r = await db.AttendanceRecords.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ExternalId == request.ExternalId && !a.IsDeleted, ct);
        return r is null ? Result<AttendanceRecordDto>.Failure(Error.NotFound("Record not found."))
            : Result<AttendanceRecordDto>.Success(AttendanceMapping.ToDto(r));
    }
}

public sealed class MarkAttendanceCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<MarkAttendanceCommand, Result<AttendanceRecordDto>>
{
    public async Task<Result<AttendanceRecordDto>> Handle(MarkAttendanceCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<AttendanceRecordDto>.Failure(Error.Forbidden("Tenant required."));
        var body = request.Request;
        if (!DateOnly.TryParse(body.Date, out var date)) return Result<AttendanceRecordDto>.Failure(Error.Validation("Invalid date."));

        var existing = await db.AttendanceRecords.FirstOrDefaultAsync(a =>
            a.TenantId == tenant.TenantId && a.EntityType == body.EntityType &&
            a.EntityExternalId == body.EntityId && a.Date == date && !a.IsDeleted, ct);

        if (existing is not null)
        {
            existing.Status = body.Status;
            existing.CheckIn = body.CheckIn;
            existing.CheckOut = body.CheckOut;
            existing.Remarks = body.Remarks;
            existing.EntityName = body.Name;
            existing.ClassName = body.Class;
            await db.SaveChangesAsync(ct);
            return Result<AttendanceRecordDto>.Success(AttendanceMapping.ToDto(existing));
        }

        var record = new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            EntityType = body.EntityType,
            EntityExternalId = body.EntityId,
            EntityName = body.Name,
            ClassName = body.Class,
            Date = date,
            Status = body.Status,
            CheckIn = body.CheckIn,
            CheckOut = body.CheckOut,
            Remarks = body.Remarks,
        };
        db.AttendanceRecords.Add(record);
        await db.SaveChangesAsync(ct);
        return Result<AttendanceRecordDto>.Success(AttendanceMapping.ToDto(record));
    }
}

public sealed class BulkMarkAttendanceCommandHandler(ISender sender)
    : IRequestHandler<BulkMarkAttendanceCommand, Result<IReadOnlyList<AttendanceRecordDto>>>
{
    public async Task<Result<IReadOnlyList<AttendanceRecordDto>>> Handle(BulkMarkAttendanceCommand request, CancellationToken ct)
    {
        var results = new List<AttendanceRecordDto>();
        foreach (var item in request.Request.Records)
        {
            var mark = item with { Date = request.Request.Date, Class = item.Class ?? request.Request.Class };
            var result = await sender.Send(new MarkAttendanceCommand(mark), ct);
            if (!result.IsSuccess) return Result<IReadOnlyList<AttendanceRecordDto>>.Failure(result.Error!);
            results.Add(result.Value!);
        }

        return Result<IReadOnlyList<AttendanceRecordDto>>.Success(results);
    }
}

public sealed class GetStudentAttendanceQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetStudentAttendanceQuery, Result<IReadOnlyList<AttendanceRecordDto>>>
{
    public async Task<Result<IReadOnlyList<AttendanceRecordDto>>> Handle(GetStudentAttendanceQuery request, CancellationToken ct)
    {
        var query = db.AttendanceRecords.AsNoTracking()
            .Where(a => !a.IsDeleted && a.EntityType == "student" && a.EntityExternalId == request.StudentId);
        if (request.From.HasValue) query = query.Where(a => a.Date >= request.From.Value);
        if (request.To.HasValue) query = query.Where(a => a.Date <= request.To.Value);
        var items = await query.OrderByDescending(a => a.Date).ToListAsync(ct);
        return Result<IReadOnlyList<AttendanceRecordDto>>.Success(items.Select(AttendanceMapping.ToDto).ToList());
    }
}
