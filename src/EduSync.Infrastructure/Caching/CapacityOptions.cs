namespace EduSync.Infrastructure.Caching;

/// <summary>
/// Documented deployment target; used for config validation and ops docs (single-database mode).
/// </summary>
public sealed class CapacityOptions
{
    public int TargetSchools { get; set; } = 200;

    public int MaxConcurrentUsersPerSchool { get; set; } = 300;

    public int ParentDailyActivePerSchool { get; set; } = 500;

    /// <summary>All reads and writes use <c>DefaultConnection</c> when true (required for your deployment).</summary>
    public bool SingleDatabase { get; set; } = true;
}
