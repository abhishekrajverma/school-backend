namespace EduSync.Modules.Admissions.Domain;

public sealed class AdmissionApproval
{
    public Guid Id { get; set; }
    public Guid AdmissionApplicationId { get; set; }
    public Guid ApprovedByUserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime DecidedAt { get; set; }

    public AdmissionApplication Application { get; set; } = null!;
}
