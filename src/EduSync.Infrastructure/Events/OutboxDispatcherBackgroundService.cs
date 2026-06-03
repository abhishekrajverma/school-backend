using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Events.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Events;

public sealed class OutboxDispatcherBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options.Value;
        if (!opts.Enabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchBatchAsync(opts, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Outbox dispatch batch failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(opts.PollIntervalSeconds), stoppingToken);
        }
    }

    private async Task DispatchBatchAsync(OutboxOptions opts, CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        var handlers = scope.ServiceProvider.GetServices<IIntegrationEventHandler>().ToList();

        var batch = await db.OutboxMessages
            .Where(m => m.Status == OutboxStatuses.Pending && m.Attempts < opts.MaxAttempts)
            .OrderBy(m => m.CreatedAt)
            .Take(opts.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in batch)
        {
            try
            {
                foreach (var handler in handlers.Where(h => h.CanHandle(message.EventType)))
                {
                    await handler.HandleAsync(message.EventType, message.Payload, message.TenantId, cancellationToken);
                }

                message.Status = OutboxStatuses.Processed;
                message.ProcessedAt = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.Error = ex.Message;
                if (message.Attempts >= opts.MaxAttempts)
                {
                    message.Status = OutboxStatuses.Failed;
                }
            }
        }

        if (batch.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
