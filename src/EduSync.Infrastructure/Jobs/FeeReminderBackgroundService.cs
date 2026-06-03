using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Jobs;

public sealed class FeeReminderBackgroundService(
    IServiceProvider serviceProvider,
    IOptions<ScheduledJobsOptions> options,
    ILogger<FeeReminderBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var opts = options.Value;
        if (!opts.Enabled || opts.UseHangfire)
        {
            logger.LogInformation("Fee reminder background service skipped (Hangfire or disabled).");
            return;
        }

        var delay = TimeSpan.FromHours(Math.Max(1, opts.FeeReminderIntervalHours));
        logger.LogInformation("Fee reminder scheduler started (interval {Hours}h).", delay.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var scheduler = serviceProvider.GetRequiredService<IFeeReminderScheduler>();
                await scheduler.RunAllTenantsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fee reminder scheduled run failed.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }
}
