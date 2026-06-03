namespace EduSync.Infrastructure.Events;

public sealed class OutboxOptions
{
    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int MaxAttempts { get; set; } = 5;
}
