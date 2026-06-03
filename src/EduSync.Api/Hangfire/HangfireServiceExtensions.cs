using EduSync.Infrastructure.Hangfire;
using EduSync.Infrastructure.Jobs;
using Hangfire;
using Hangfire.SqlServer;

namespace EduSync.Api.Hangfire;

public static class HangfireServiceExtensions
{
    public static IServiceCollection AddEduSyncHangfire(this IServiceCollection services, string connectionString)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                SchemaName = "hangfire",
                PrepareSchemaIfNecessary = true,
            }));

        services.AddHangfireServer();
        services.AddScoped<HangfireFeeReminderJob>();
        services.AddScoped<HangfireBulkImportJob>();
        return services;
    }

    public static void RegisterFeeReminderRecurringJob(IConfiguration configuration)
    {
        var scheduled = configuration.GetSection("ScheduledJobs").Get<ScheduledJobsOptions>() ?? new();
        if (!scheduled.UseHangfire) return;

        var hours = Math.Max(1, scheduled.FeeReminderIntervalHours);
        var cron = hours >= 24 ? Cron.Daily() : $"0 */{hours} * * *";
        RecurringJob.AddOrUpdate<HangfireFeeReminderJob>(
            "fee-reminders-all-tenants",
            job => job.ExecuteAsync(),
            cron);
    }
}
