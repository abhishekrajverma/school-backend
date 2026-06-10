namespace EduSync.Modules.Admissions.Domain;

public static class AdmissionSources
{
    public const string Online = "online";
    public const string Offline = "offline";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Online, Offline,
    };
}
