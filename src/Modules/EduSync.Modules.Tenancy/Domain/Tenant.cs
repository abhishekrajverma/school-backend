namespace EduSync.Modules.Tenancy.Domain;

public sealed class Tenant
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SchoolEmail { get; set; }
    public string? LogoUrl { get; set; }
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public DateTime CreatedAt { get; set; }

    public TenantSubscription? Subscription { get; set; }
    public ICollection<AcademicYear> AcademicYears { get; set; } = [];
}
