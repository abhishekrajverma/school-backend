using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Audit.Application;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Audit;

internal static class AuditMapping
{
    public static AuditLogDto ToDto(Modules.Audit.Domain.AuditLogEntry e) => new(
        e.ExternalId,
        e.Action,
        e.Method,
        e.Path,
        e.StatusCode,
        e.UserEmail,
        e.EntityType,
        e.EntityId,
        e.OccurredAt);
}

public sealed class ListAuditLogsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
{
    public async Task<Result<PaginatedList<AuditLogDto>>> Handle(ListAuditLogsQuery request, CancellationToken ct)
    {
        var query = db.AuditLogEntries.AsNoTracking().Where(a => !a.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (!string.IsNullOrWhiteSpace(request.Path))
        {
            query = query.Where(a => a.Path.Contains(request.Path));
        }

        query = query.OrderByDescending(a => a.OccurredAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(AuditMapping.ToDto).ToList();
        return Result<PaginatedList<AuditLogDto>>.Success(
            PaginatedList<AuditLogDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}
