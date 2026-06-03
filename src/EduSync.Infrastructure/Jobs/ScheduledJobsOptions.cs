namespace EduSync.Infrastructure.Jobs;

public sealed class ScheduledJobsOptions
{
    public bool Enabled { get; set; } = true;
    public int FeeReminderIntervalHours { get; set; } = 24;
    /// <summary>When true, Hangfire runs fee reminders; hosted BackgroundService is disabled.</summary>
    public bool UseHangfire { get; set; }
}
