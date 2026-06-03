using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Webhooks.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Events;

public sealed class WebhookIntegrationEventHandler(
    EduSyncDbContext db,
    IHttpClientFactory httpClientFactory,
    ILogger<WebhookIntegrationEventHandler> logger) : IIntegrationEventHandler
{
    public bool CanHandle(string eventType) => true;

    public async Task HandleAsync(
        string eventType,
        string payload,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!tenantId.HasValue)
        {
            return;
        }

        var subscriptions = await db.WebhookSubscriptions.IgnoreQueryFilters().AsNoTracking()
            .Where(s => !s.IsDeleted && s.IsActive && s.TenantId == tenantId.Value)
            .ToListAsync(cancellationToken);

        var matching = subscriptions.Where(s => MatchesEventType(s.EventTypes, eventType)).ToList();
        if (matching.Count == 0)
        {
            return;
        }

        var client = httpClientFactory.CreateClient("webhooks");
        foreach (var sub in matching)
        {
            var delivery = new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                SubscriptionId = sub.Id,
                TenantId = sub.TenantId,
                EventType = eventType,
                Payload = payload,
                Status = WebhookDeliveryStatuses.Pending,
                CreatedAt = DateTime.UtcNow,
            };
            db.WebhookDeliveries.Add(delivery);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, sub.Url)
                {
                    Content = JsonContent.Create(new { type = eventType, data = payload, sentAt = DateTime.UtcNow }),
                };
                request.Headers.Add("X-EduSync-Event", eventType);
                if (!string.IsNullOrWhiteSpace(sub.Secret))
                {
                    var signature = ComputeSignature(sub.Secret, payload);
                    request.Headers.Add("X-EduSync-Signature", signature);
                }

                var response = await client.SendAsync(request, cancellationToken);
                delivery.StatusCode = (int)response.StatusCode;
                if (response.IsSuccessStatusCode)
                {
                    delivery.Status = WebhookDeliveryStatuses.Delivered;
                    delivery.DeliveredAt = DateTime.UtcNow;
                }
                else
                {
                    delivery.Status = WebhookDeliveryStatuses.Failed;
                    delivery.Error = $"HTTP {(int)response.StatusCode}";
                    delivery.Attempts = 1;
                }
            }
            catch (Exception ex)
            {
                delivery.Status = WebhookDeliveryStatuses.Failed;
                delivery.Error = ex.Message;
                delivery.Attempts = 1;
                logger.LogWarning(ex, "Webhook delivery failed for {Url}", sub.Url);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool MatchesEventType(string configured, string eventType)
    {
        if (configured == "*" || string.IsNullOrWhiteSpace(configured))
        {
            return true;
        }

        return configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(t => t.Equals(eventType, StringComparison.OrdinalIgnoreCase));
    }

    private static string ComputeSignature(string secret, string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(secret + payload);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
