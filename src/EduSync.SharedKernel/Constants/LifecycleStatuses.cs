namespace EduSync.SharedKernel.Constants;

public static class LifecycleStatuses
{
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Alumni = "alumni";
    public const string Transferred = "transferred";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Active, Inactive, Alumni, Transferred,
    };

    public static bool IsPromotable(string status) =>
        string.Equals(status, Active, StringComparison.OrdinalIgnoreCase);
}
