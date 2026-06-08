namespace EduSync.Modules.Company.Domain;

public sealed class SchoolEnquiry
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string SchoolName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? PlanKey { get; set; }
    public string Status { get; set; } = EnquiryStatuses.New;
    public string? Notes { get; set; }
    public string? TenantExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public static class EnquiryStatuses
{
    public const string New = "new";
    public const string Contacted = "contacted";
    public const string Converted = "converted";
    public const string Rejected = "rejected";
}
