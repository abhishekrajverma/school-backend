namespace EduSync.SharedKernel.Constants;

public static class EnrollmentStatuses
{
    public const string Enrolled = "enrolled";
    public const string Promoted = "promoted";
    public const string Withdrawn = "withdrawn";
    public const string Transferred = "transferred";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Enrolled, Promoted, Withdrawn, Transferred,
    };

    public static bool IsOpen(string status) =>
        string.Equals(status, Enrolled, StringComparison.OrdinalIgnoreCase);
}
