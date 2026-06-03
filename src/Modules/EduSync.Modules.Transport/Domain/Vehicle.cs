using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Transport.Domain;

public sealed class Vehicle : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "bus";
    public int Capacity { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
    public string? RouteExternalId { get; set; }
    public string? RouteName { get; set; }
    public DateOnly InsuranceExpiry { get; set; }
    public DateOnly FitnessExpiry { get; set; }
    public int CurrentStudents { get; set; }
    public string Status { get; set; } = "active";
    public string GpsStatus { get; set; } = "offline";
    public string? LastLocation { get; set; }
}
