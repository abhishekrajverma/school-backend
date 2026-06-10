using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Assignments.Domain;

public sealed class StudentAssignment : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentExternalId { get; set; } = string.Empty;
    public string Status { get; set; } = StudentAssignmentStatuses.Pending;
    public string? SubmissionText { get; set; }
    public decimal? Score { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public Assignment Assignment { get; set; } = null!;
}

public static class StudentAssignmentStatuses
{
    public const string Pending = "pending";
    public const string Submitted = "submitted";
    public const string Graded = "graded";
}
