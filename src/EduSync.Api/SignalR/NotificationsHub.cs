using System.Security.Claims;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Api.SignalR;

[Authorize]
public sealed class NotificationsHub(
    ITenantContext tenantContext,
    EduSyncDbContext db) : Hub
{
    public const string HubPath = "/hubs/notifications";
    public const string EventCreated = "notification.created";

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var tenantKey = http?.Request.Query["tenant_id"].FirstOrDefault()
            ?? http?.Request.Headers[HttpHeaders.TenantId].FirstOrDefault()
            ?? tenantContext.TenantExternalId;

        var userIdClaim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (!string.IsNullOrWhiteSpace(tenantKey)
            && userIdClaim is not null
            && Guid.TryParse(userIdClaim, out var userId)
            && tenantContext.TenantId.HasValue)
        {
            var isMember = await db.TenantMemberships.AsNoTracking()
                .AnyAsync(m => m.TenantId == tenantContext.TenantId && m.UserId == userId && m.IsActive);
            if (!isMember)
            {
                Context.Abort();
                return;
            }
        }

        if (!string.IsNullOrWhiteSpace(tenantKey))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantKey));
        }

        var audience = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (!string.IsNullOrWhiteSpace(tenantKey) && !string.IsNullOrWhiteSpace(audience))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AudienceGroup(tenantKey, audience));
        }

        if (!string.IsNullOrWhiteSpace(tenantKey) && !string.IsNullOrWhiteSpace(userIdClaim))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(tenantKey, userIdClaim));
        }

        await base.OnConnectedAsync();
    }

    public static string TenantGroup(string tenantExternalId) => $"tenant:{tenantExternalId}";
    public static string AudienceGroup(string tenantExternalId, string audience) => $"tenant:{tenantExternalId}:audience:{audience}";
    public static string UserGroup(string tenantExternalId, string userId) => $"tenant:{tenantExternalId}:user:{userId}";
}
