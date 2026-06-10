using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Admissions.Domain;

public sealed partial class Registration : BranchEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty;
    public string Source { get; set; } = RegistrationSources.Online;
    public string Status { get; set; } = RegistrationStatuses.Draft;
    public Guid AcademicYearId { get; set; }
    public string? ClassSought { get; set; }
    public string ApplicantFirstName { get; set; } = string.Empty;
    public string ApplicantLastName { get; set; } = string.Empty;
    public string? ApplicantEmail { get; set; }
    public string? ApplicantPhone { get; set; }
    public string FormDataJson { get; set; } = "{}";
    public DateTime? SubmittedAt { get; set; }
}
