using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Webhooks.Application;
using EduSync.Modules.Webhooks.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Webhooks;

internal static class WebhookMapping
{
    public static WebhookSubscriptionDto ToDto(WebhookSubscription s) => new(
        s.ExternalId, s.Url, s.EventTypes, s.IsActive, s.CreatedAt);

    public static WebhookDeliveryDto ToDto(WebhookDelivery d) => new(
        d.ExternalId, d.EventType, d.Status, d.StatusCode, d.Attempts, d.CreatedAt, d.DeliveredAt, d.Error);
}

public sealed class ListWebhookSubscriptionsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListWebhookSubscriptionsQuery, Result<PaginatedList<WebhookSubscriptionDto>>>
{
    public async Task<Result<PaginatedList<WebhookSubscriptionDto>>> Handle(
        ListWebhookSubscriptionsQuery request,
        CancellationToken ct)
    {
        var query = db.WebhookSubscriptions.AsNoTracking().Where(s => !s.IsDeleted).OrderByDescending(s => s.CreatedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(WebhookMapping.ToDto).ToList();
        return Result<PaginatedList<WebhookSubscriptionDto>>.Success(
            PaginatedList<WebhookSubscriptionDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class CreateWebhookSubscriptionCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateWebhookSubscriptionCommand, Result<WebhookSubscriptionDto>>
{
    public async Task<Result<WebhookSubscriptionDto>> Handle(CreateWebhookSubscriptionCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<WebhookSubscriptionDto>.Failure(Error.Forbidden("Tenant required."));
        }

        var body = request.Request;
        if (!Uri.TryCreate(body.Url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return Result<WebhookSubscriptionDto>.Failure(Error.Validation("Valid http(s) webhook URL is required."));
        }

        var sub = new WebhookSubscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            Url = body.Url.Trim(),
            Secret = body.Secret,
            EventTypes = string.IsNullOrWhiteSpace(body.EventTypes) ? "*" : body.EventTypes.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        db.WebhookSubscriptions.Add(sub);
        await db.SaveChangesAsync(ct);
        return Result<WebhookSubscriptionDto>.Success(WebhookMapping.ToDto(sub));
    }
}

public sealed class DeleteWebhookSubscriptionCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteWebhookSubscriptionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteWebhookSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await db.WebhookSubscriptions.FirstOrDefaultAsync(
            s => s.ExternalId == request.ExternalId && !s.IsDeleted, ct);
        if (sub is null)
        {
            return Result<bool>.Failure(Error.NotFound("Webhook not found."));
        }

        sub.IsDeleted = true;
        sub.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}

public sealed class ListWebhookDeliveriesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListWebhookDeliveriesQuery, Result<PaginatedList<WebhookDeliveryDto>>>
{
    public async Task<Result<PaginatedList<WebhookDeliveryDto>>> Handle(
        ListWebhookDeliveriesQuery request,
        CancellationToken ct)
    {
        var query = db.WebhookDeliveries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(d => d.Status == request.Status);
        }

        query = query.OrderByDescending(d => d.CreatedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(WebhookMapping.ToDto).ToList();
        return Result<PaginatedList<WebhookDeliveryDto>>.Success(
            PaginatedList<WebhookDeliveryDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}
