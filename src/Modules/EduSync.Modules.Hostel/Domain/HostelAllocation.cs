using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Hostel.Domain;

public sealed class HostelAllocation : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string RoomExternalId { get; set; } = string.Empty;
    public string StudentExternalId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateOnly AllocatedOn { get; set; }
    public string Status { get; set; } = "active";

    public HostelRoom Room { get; set; } = null!;
}
