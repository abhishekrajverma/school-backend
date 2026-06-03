using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Jobs.Domain;
using EduSync.Modules.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Jobs;

public interface IFeeReminderScheduler
{
    Task RunAllTenantsAsync(CancellationToken cancellationToken = default);
}

public sealed class FeeReminderScheduler(
    IServiceProvider serviceProvider,
    ILogger<FeeReminderScheduler> logger) : IFeeReminderScheduler
{
    public async Task RunAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        var tenants = await db.Tenants.AsNoTracking()
            .Where(t => t.Status == TenantStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            await using var tenantScope = serviceProvider.CreateAsyncScope();
            var tenantContext = (TenantContext)tenantScope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.Set(tenant.Id, tenant.Slug, tenant.ExternalId);

            var tenantDb = tenantScope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
            var job = tenantScope.ServiceProvider.GetRequiredService<IFeeReminderJob>();

            var execution = new JobExecution
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                JobType = FeeReminderJob.JobType,
                Status = "running",
                StartedAt = DateTime.UtcNow,
            };
            tenantDb.JobExecutions.Add(execution);
            await tenantDb.SaveChangesAsync(cancellationToken);

            try
            {
                var count = await job.RunAsync(cancellationToken);
                execution.Status = "completed";
                execution.CompletedAt = DateTime.UtcNow;
                execution.ItemsProcessed = count;
                execution.Message = $"Scheduled run: {count} notification(s).";
            }
            catch (Exception ex)
            {
                execution.Status = "failed";
                execution.CompletedAt = DateTime.UtcNow;
                execution.Message = ex.Message;
                logger.LogWarning(ex, "Fee reminder failed for tenant {TenantId}", tenant.ExternalId);
            }

            await tenantDb.SaveChangesAsync(cancellationToken);
        }
    }
}
