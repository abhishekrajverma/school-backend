namespace EduSync.Modules.Tenancy.Domain;

public sealed class TenantSubscription
{
    public Guid TenantId { get; set; }
    public string PlanId { get; set; } = "starter";
    public int SeatLimit { get; set; } = 50;
    public DateTime? ExpiresAt { get; set; }
    public string FeatureFlagsJson { get; set; } = "{}";

    public Tenant Tenant { get; set; } = null!;
}
