using System.Text.Json;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Events.Domain;
using Microsoft.AspNetCore.Http;

namespace EduSync.Infrastructure.Events;

public static class IntegrationEventFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static IntegrationEvent Create(
        string eventType,
        object payload,
        ITenantContext tenant,
        IRegionContext? region = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        var correlationId = httpContextAccessor?.HttpContext?.Items[SharedKernel.Constants.HttpHeaders.CorrelationId]?.ToString()
            ?? httpContextAccessor?.HttpContext?.TraceIdentifier;

        return new IntegrationEvent(
            eventType,
            tenant.TenantId,
            JsonSerializer.Serialize(payload, JsonOptions),
            region?.CurrentRegion,
            correlationId);
    }
}
