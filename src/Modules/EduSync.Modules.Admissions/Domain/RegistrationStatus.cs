namespace EduSync.Modules.Admissions.Domain;

public static class RegistrationStatuses
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string Verified = "verified";
    public const string Cancelled = "cancelled";
    public const string Converted = "converted";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft, Submitted, Verified, Cancelled, Converted,
    };
}

public static class RegistrationSources
{
    public const string Online = "online";
    public const string Offline = "offline";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Online, Offline,
    };
}
