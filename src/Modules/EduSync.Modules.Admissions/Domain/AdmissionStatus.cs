namespace EduSync.Modules.Admissions.Domain;

public static class AdmissionStatuses
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string UnderReview = "under_review";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Waitlisted = "waitlisted";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft, Submitted, UnderReview, Approved, Rejected, Waitlisted,
    };
}
