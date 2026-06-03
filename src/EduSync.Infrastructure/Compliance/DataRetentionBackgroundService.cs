using EduSync.Infrastructure.Application.Compliance;
using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Compliance;

public sealed class DataRetentionBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<RetentionOptions> options,
    ILogger<DataRetentionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunCleanupAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Data retention cleanup failed.");
            }

            await Task.Delay(TimeSpan.FromHours(options.Value.RunIntervalHours), stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        var tenants = await db.Tenants.AsNoTracking()
            .Where(t => t.Status == TenantStatus.Active)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        foreach (var tenantId in tenants)
        {
            var result = await RetentionCleanupExecutor.RunForTenantAsync(db, tenantId, cancellationToken);
            if (result.AuditLogsDeleted + result.OutboxDeleted + result.WebhookDeliveriesDeleted > 0)
            {
                logger.LogInformation(
                    "Retention tenant {TenantId}: audit={Audit}, outbox={Outbox}, webhooks={Webhooks}",
                    tenantId,
                    result.AuditLogsDeleted,
                    result.OutboxDeleted,
                    result.WebhookDeliveriesDeleted);
            }
        }
    }
}
