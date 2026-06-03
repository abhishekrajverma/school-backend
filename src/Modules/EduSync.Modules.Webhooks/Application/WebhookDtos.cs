using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Webhooks.Application;

public sealed record WebhookSubscriptionDto(
    string Id,
    string Url,
    string EventTypes,
    bool IsActive,
    DateTime CreatedAt);

public sealed record CreateWebhookRequest(string Url, string? Secret, string EventTypes);

public sealed record WebhookDeliveryDto(
    string Id,
    string EventType,
    string Status,
    int StatusCode,
    int Attempts,
    DateTime CreatedAt,
    DateTime? DeliveredAt,
    string? Error);

public sealed record ListWebhookSubscriptionsQuery(PaginationQuery Pagination)
    : IRequest<Result<PaginatedList<WebhookSubscriptionDto>>>;

public sealed record CreateWebhookSubscriptionCommand(CreateWebhookRequest Request)
    : IRequest<Result<WebhookSubscriptionDto>>;

public sealed record DeleteWebhookSubscriptionCommand(string ExternalId)
    : IRequest<Result<bool>>;

public sealed record ListWebhookDeliveriesQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<WebhookDeliveryDto>>>;
