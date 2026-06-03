using EduSync.Infrastructure.Jobs;

namespace EduSync.Infrastructure.Hangfire;

public sealed class HangfireFeeReminderJob(IFeeReminderScheduler scheduler)
{
    public Task ExecuteAsync() => scheduler.RunAllTenantsAsync();
}
