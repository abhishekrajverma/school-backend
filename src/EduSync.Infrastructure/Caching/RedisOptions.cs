namespace EduSync.Infrastructure.Caching;

public sealed class RedisOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = "localhost:6379";
    public int TenantCacheMinutes { get; set; } = 10;
    public int DashboardCacheSeconds { get; set; } = 60;
    /// <summary>Per-tenant requests/minute. Default supports ~300 concurrent users per school (bursty SPA).</summary>
    public int RateLimitPerMinute { get; set; } = 8000;
}
