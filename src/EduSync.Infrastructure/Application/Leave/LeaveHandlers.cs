using System.Text.Json;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Leave.Application;
using EduSync.Modules.Leave.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Leave;

internal static class LeaveMapping
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static LeaveRequestDto ToDto(LeaveRequest r) => new(
        r.ExternalId, r.EmployeeExternalId, r.EmployeeName, r.Department, r.LeaveType,
        r.StartDate.ToString("yyyy-MM-dd"), r.EndDate.ToString("yyyy-MM-dd"), r.Days, r.Reason, r.Status,
        r.AppliedOn.ToString("yyyy-MM-dd"), r.ApprovedBy, r.ApprovedOn?.ToString("yyyy-MM-dd"),
        ParseProof(r.ProofDocumentJson));

    private static LeaveProofDocumentDto? ParseProof(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<LeaveProofDocumentDto>(json, Json); }
        catch { return null; }
    }

    public static string? SerializeProof(LeaveProofDocumentDto? doc) =>
        doc is null ? null : JsonSerializer.Serialize(doc, Json);
}

public sealed class ListLeaveRequestsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListLeaveRequestsQuery, Result<PaginatedList<LeaveRequestDto>>>
{
    public async Task<Result<PaginatedList<LeaveRequestDto>>> Handle(ListLeaveRequestsQuery request, CancellationToken ct)
    {
        var query = db.LeaveRequests.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.EmployeeId)) query = query.Where(x => x.EmployeeExternalId == request.EmployeeId);
        query = query.OrderByDescending(x => x.AppliedOn);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(LeaveMapping.ToDto).ToList();
        return Result<PaginatedList<LeaveRequestDto>>.Success(
            PaginatedList<LeaveRequestDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetLeaveByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetLeaveByIdQuery, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(GetLeaveByIdQuery request, CancellationToken ct)
    {
        var r = await db.LeaveRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return r is null ? Result<LeaveRequestDto>.Failure(Error.NotFound("Leave request not found."))
            : Result<LeaveRequestDto>.Success(LeaveMapping.ToDto(r));
    }
}

public sealed class CreateLeaveCommandHandler(EduSyncDbContext db, ITenantContext tenant, ICurrentUserContext user)
    : IRequestHandler<CreateLeaveCommand, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(CreateLeaveCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<LeaveRequestDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.StartDate, out var start) || !DateOnly.TryParse(b.EndDate, out var end))
            return Result<LeaveRequestDto>.Failure(Error.Validation("Invalid date range."));

        string employeeId = b.EmployeeId ?? user.UserExternalId ?? "";
        string employeeName = b.EmployeeName ?? "";
        string department = b.Department ?? "";

        if (string.IsNullOrWhiteSpace(employeeId))
            return Result<LeaveRequestDto>.Failure(Error.Validation("Employee is required."));

        if (string.IsNullOrWhiteSpace(employeeName) || string.IsNullOrWhiteSpace(department))
        {
            var teacher = await db.Teachers.AsNoTracking()
                .FirstOrDefaultAsync(t => t.ExternalId == employeeId && !t.IsDeleted, ct);
            if (teacher is not null)
            {
                employeeName = $"{teacher.FirstName} {teacher.LastName}";
                department = teacher.Department;
            }
        }

        var days = end.DayNumber - start.DayNumber + 1;
        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            EmployeeExternalId = employeeId,
            EmployeeName = employeeName,
            Department = department,
            LeaveType = b.LeaveType,
            StartDate = start,
            EndDate = end,
            Days = Math.Max(1, days),
            Reason = b.Reason,
            Status = "pending",
            AppliedOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ProofDocumentJson = LeaveMapping.SerializeProof(b.ProofDocument),
        };
        db.LeaveRequests.Add(entity);
        await db.SaveChangesAsync(ct);
        return Result<LeaveRequestDto>.Success(LeaveMapping.ToDto(entity));
    }
}

public sealed class ApproveLeaveCommandHandler(EduSyncDbContext db)
    : IRequestHandler<ApproveLeaveCommand, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(ApproveLeaveCommand request, CancellationToken ct)
    {
        var r = await db.LeaveRequests.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (r is null) return Result<LeaveRequestDto>.Failure(Error.NotFound("Leave request not found."));
        r.Status = "approved";
        r.ApprovedBy = request.ApprovedBy ?? "Admin";
        r.ApprovedOn = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Result<LeaveRequestDto>.Success(LeaveMapping.ToDto(r));
    }
}

public sealed class RejectLeaveCommandHandler(EduSyncDbContext db)
    : IRequestHandler<RejectLeaveCommand, Result<LeaveRequestDto>>
{
    public async Task<Result<LeaveRequestDto>> Handle(RejectLeaveCommand request, CancellationToken ct)
    {
        var r = await db.LeaveRequests.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (r is null) return Result<LeaveRequestDto>.Failure(Error.NotFound("Leave request not found."));
        r.Status = "rejected";
        r.ApprovedBy = request.ApprovedBy ?? "Admin";
        r.ApprovedOn = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Result<LeaveRequestDto>.Success(LeaveMapping.ToDto(r));
    }
}
