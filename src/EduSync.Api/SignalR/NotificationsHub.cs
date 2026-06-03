using System.Security.Claims;
using EduSync.Infrastructure.Tenancy;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EduSync.Api.SignalR;

[Authorize]
public sealed class NotificationsHub(ITenantContext tenantContext) : Hub
{
    public const string HubPath = "/hubs/notifications";
    public const string EventCreated = "notification.created";

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var tenantKey = http?.Request.Query["tenant_id"].FirstOrDefault()
            ?? http?.Request.Headers[HttpHeaders.TenantId].FirstOrDefault()
            ?? tenantContext.TenantExternalId;

        if (!string.IsNullOrWhiteSpace(tenantKey))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroup(tenantKey));
        }

        var audience = Context.User?.FindFirstValue(ClaimTypes.Role);
        if (!string.IsNullOrWhiteSpace(tenantKey) && !string.IsNullOrWhiteSpace(audience))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AudienceGroup(tenantKey, audience));
        }

        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");
        if (!string.IsNullOrWhiteSpace(tenantKey) && !string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(tenantKey, userId));
        }

        await base.OnConnectedAsync();
    }

    public static string TenantGroup(string tenantExternalId) => $"tenant:{tenantExternalId}";
    public static string AudienceGroup(string tenantExternalId, string audience) => $"tenant:{tenantExternalId}:audience:{audience}";
    public static string UserGroup(string tenantExternalId, string userId) => $"tenant:{tenantExternalId}:user:{userId}";
}
