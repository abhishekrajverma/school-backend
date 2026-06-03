using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Transport.Domain;

public sealed class TransportAssignment : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string StudentExternalId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string RouteExternalId { get; set; } = string.Empty;
    public int PickupStopOrder { get; set; }
    public string Shift { get; set; } = "both";
    public DateOnly EnrolledSince { get; set; }
    public string Status { get; set; } = "active";
    public string? SeatNumber { get; set; }
}
