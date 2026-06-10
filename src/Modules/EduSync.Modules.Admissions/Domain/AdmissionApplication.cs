using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Admissions.Domain;

public sealed partial class AdmissionApplication : BranchEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string ApplicationNo { get; set; } = string.Empty;
    public Guid? RegistrationId { get; set; }
    public Guid AcademicYearId { get; set; }
    public string Source { get; set; } = AdmissionSources.Online;
    public string Status { get; set; } = AdmissionStatuses.Draft;
    public string CurrentStep { get; set; } = "personal";
    public string FormDataJson { get; set; } = "{}";
    public string? DocumentsJson { get; set; }
    public string? ApplicantName { get; set; }
    public string? ClassSought { get; set; }
    public string? AcademicSession { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? ApprovedStudentExternalId { get; set; }

    public Registration? Registration { get; set; }
    public ICollection<AdmissionApproval> Approvals { get; set; } = [];
}
