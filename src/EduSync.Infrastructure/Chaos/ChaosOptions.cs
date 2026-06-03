namespace EduSync.Infrastructure.Chaos;

public sealed class ChaosOptions
{
    public bool Enabled { get; set; }
    public double FailureRate { get; set; } = 0.05;
    public int MaxLatencyMs { get; set; } = 500;
    public bool AllowInProduction { get; set; }
}
