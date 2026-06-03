namespace EduSync.Infrastructure.Audit;

public sealed class AuditOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogGetRequests { get; set; }
}
