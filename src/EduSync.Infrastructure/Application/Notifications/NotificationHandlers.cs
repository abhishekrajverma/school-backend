using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Realtime;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Events.Domain;
using Microsoft.AspNetCore.Http;
using EduSync.Modules.Notifications.Application;
using EduSync.Modules.Notifications.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Notifications;

public static class NotificationMapping
{
    public static NotificationDto ToDto(Notification n) => new(
        n.ExternalId, n.Title, n.Message, n.Type, n.TargetAudience, n.SentAt, n.ReadCount, n.TotalRecipients);
}

public sealed class ListNotificationsQueryHandler(IReadDbContextFactory dbFactory)
    : IRequestHandler<ListNotificationsQuery, Result<PaginatedList<NotificationDto>>>
{
    public async Task<Result<PaginatedList<NotificationDto>>> Handle(ListNotificationsQuery request, CancellationToken ct)
    {
        await using var db = dbFactory.CreateDbContext();
        var query = db.Notifications.AsNoTracking().Where(n => !n.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.TargetAudience)) query = query.Where(n => n.TargetAudience == request.TargetAudience);
        query = query.OrderByDescending(n => n.SentAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(NotificationMapping.ToDto).ToList();
        return Result<PaginatedList<NotificationDto>>.Success(
            PaginatedList<NotificationDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetNotificationByIdQueryHandler(IReadDbContextFactory dbFactory)
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(GetNotificationByIdQuery request, CancellationToken ct)
    {
        await using var db = dbFactory.CreateDbContext();
        var n = await db.Notifications.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return n is null ? Result<NotificationDto>.Failure(Error.NotFound("Notification not found."))
            : Result<NotificationDto>.Success(NotificationMapping.ToDto(n));
    }
}

public sealed class CreateNotificationCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    INotificationRealtimePublisher realtime,
    IIntegrationEventCollector events,
    IRegionContext region,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<CreateNotificationCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(CreateNotificationCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<NotificationDto>.Failure(Error.Forbidden("Tenant required."));
        var body = request.Request;
        var n = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            Title = body.Title,
            Message = body.Message,
            Type = body.Type,
            TargetAudience = body.TargetAudience,
            SentAt = DateTime.UtcNow,
            TotalRecipients = body.TotalRecipients,
        };
        db.Notifications.Add(n);
        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.NotificationCreated,
            new { n.ExternalId, n.Title, n.TargetAudience },
            tenant,
            region,
            httpContextAccessor));
        await db.SaveChangesAsync(ct);
        var dto = NotificationMapping.ToDto(n);
        if (!string.IsNullOrWhiteSpace(tenant.TenantExternalId))
        {
            await realtime.PublishCreatedAsync(tenant.TenantExternalId, n.TargetAudience, dto, ct);
        }

        return Result<NotificationDto>.Success(dto);
    }
}

public sealed class MarkNotificationReadCommandHandler(EduSyncDbContext db)
    : IRequestHandler<MarkNotificationReadCommand, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (n is null) return Result<NotificationDto>.Failure(Error.NotFound("Notification not found."));
        n.ReadCount = Math.Min(n.ReadCount + 1, n.TotalRecipients);
        await db.SaveChangesAsync(ct);
        return Result<NotificationDto>.Success(NotificationMapping.ToDto(n));
    }
}
