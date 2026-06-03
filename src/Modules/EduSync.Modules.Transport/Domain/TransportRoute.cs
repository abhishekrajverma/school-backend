using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Transport.Domain;

public sealed class TransportRoute : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;
    public string? VehicleExternalId { get; set; }
    public string? VehicleNumber { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string StartPoint { get; set; } = string.Empty;
    public string EndPoint { get; set; } = string.Empty;
    public int TotalStops { get; set; }
    public int TotalStudents { get; set; }
    public decimal Fare { get; set; }
    public string MorningTime { get; set; } = string.Empty;
    public string EveningTime { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public string? Distance { get; set; }
    public string? StopsJson { get; set; }
}
