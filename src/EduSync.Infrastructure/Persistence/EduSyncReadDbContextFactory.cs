using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Persistence;

public sealed class EduSyncReadDbContextFactory(
    IConfiguration configuration,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<CapacityOptions> capacityOptions,
    ITenantContext tenantContext,
    IBranchContext branchContext,
    IIntegrationEventCollector eventCollector) : IReadDbContextFactory
{
    private readonly DbContextOptions<EduSyncDbContext> _options = BuildOptions(configuration, databaseOptions.Value, capacityOptions.Value);

    public EduSyncDbContext CreateDbContext() => new(_options, tenantContext, branchContext, eventCollector);

    private static DbContextOptions<EduSyncDbContext> BuildOptions(
        IConfiguration configuration,
        DatabaseOptions dbOptions,
        CapacityOptions capacityOptions)
    {
        var readConnection = configuration.GetConnectionString("ReadConnection");
        var useReplica = dbOptions.UseReadReplica
            && !capacityOptions.SingleDatabase
            && !string.IsNullOrWhiteSpace(readConnection);
        var connection = useReplica
            ? readConnection!
            : configuration.GetConnectionString("DefaultConnection")!;

        return new DbContextOptionsBuilder<EduSyncDbContext>()
            .UseSqlServer(connection, sql =>
            {
                sql.MigrationsAssembly(typeof(EduSyncDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
                sql.CommandTimeout(dbOptions.CommandTimeoutSeconds);
            })
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options;
    }
}
