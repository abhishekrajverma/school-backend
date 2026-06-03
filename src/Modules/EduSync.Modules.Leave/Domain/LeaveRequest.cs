using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Leave.Domain;

public sealed class LeaveRequest : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string EmployeeExternalId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int Days { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateOnly AppliedOn { get; set; }
    public string? ApprovedBy { get; set; }
    public DateOnly? ApprovedOn { get; set; }
    public string? ProofDocumentJson { get; set; }
}
