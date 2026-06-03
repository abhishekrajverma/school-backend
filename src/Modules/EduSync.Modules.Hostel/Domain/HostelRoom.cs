using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Hostel.Domain;

public sealed class HostelRoom : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string RoomNo { get; set; } = string.Empty;
    public string Block { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Occupied { get; set; }
    public int Floor { get; set; }
    public string Warden { get; set; } = string.Empty;
    public string Status { get; set; } = "available";
    public decimal MonthlyFee { get; set; }
}
