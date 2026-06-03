using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Notifications.Application;

public sealed record NotificationDto(
    string Id,
    string Title,
    string Message,
    string Type,
    string TargetAudience,
    DateTime SentAt,
    int ReadCount,
    int TotalRecipients);

public sealed record CreateNotificationRequest(
    string Title,
    string Message,
    string Type,
    string TargetAudience,
    int TotalRecipients);

public sealed record ListNotificationsQuery(PaginationQuery Pagination, string? TargetAudience)
    : IRequest<Result<PaginatedList<NotificationDto>>>;

public sealed record GetNotificationByIdQuery(string ExternalId) : IRequest<Result<NotificationDto>>;
public sealed record CreateNotificationCommand(CreateNotificationRequest Request) : IRequest<Result<NotificationDto>>;
public sealed record MarkNotificationReadCommand(string ExternalId) : IRequest<Result<NotificationDto>>;
