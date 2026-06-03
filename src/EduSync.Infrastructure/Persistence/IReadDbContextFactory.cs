namespace EduSync.Infrastructure.Persistence;

public interface IReadDbContextFactory
{
    EduSyncDbContext CreateDbContext();
}
