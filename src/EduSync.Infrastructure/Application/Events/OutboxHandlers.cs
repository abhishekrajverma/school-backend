using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Events.Application;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Events;

internal static class OutboxMapping
{
    public static OutboxMessageDto ToDto(Modules.Events.Domain.OutboxMessage m) => new(
        m.ExternalId,
        m.EventType,
        m.Status,
        m.TenantId,
        m.Region,
        m.CorrelationId,
        m.CreatedAt,
        m.ProcessedAt,
        m.Attempts,
        m.Error);
}

public sealed class ListOutboxMessagesQueryHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<ListOutboxMessagesQuery, Result<PaginatedList<OutboxMessageDto>>>
{
    public async Task<Result<PaginatedList<OutboxMessageDto>>> Handle(ListOutboxMessagesQuery request, CancellationToken ct)
    {
        var query = db.OutboxMessages.AsNoTracking().AsQueryable();
        if (tenant.TenantId.HasValue)
        {
            query = query.Where(m => m.TenantId == tenant.TenantId);
        }
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(m => m.Status == request.Status);
        }

        query = query.OrderByDescending(m => m.CreatedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(OutboxMapping.ToDto).ToList();
        return Result<PaginatedList<OutboxMessageDto>>.Success(
            PaginatedList<OutboxMessageDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}
