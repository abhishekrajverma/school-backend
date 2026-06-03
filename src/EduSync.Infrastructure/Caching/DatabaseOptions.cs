namespace EduSync.Infrastructure.Caching;

public sealed class DatabaseOptions
{
    /// <summary>When true and ReadConnection is set, read handlers use the replica connection. Keep false for single-database deployments.</summary>
    public bool UseReadReplica { get; set; }

    /// <summary>EF/SQL command timeout in seconds for heavy list/report queries.</summary>
    public int CommandTimeoutSeconds { get; set; } = 30;
}
