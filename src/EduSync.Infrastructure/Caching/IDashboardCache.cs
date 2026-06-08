using EduSync.Modules.Dashboard.Application;

namespace EduSync.Infrastructure.Caching;

public interface IDashboardCache
{
    Task<DashboardResponseDto?> GetAsync(Guid tenantId, string financialYear, CancellationToken cancellationToken = default);
    Task SetAsync(Guid tenantId, string financialYear, DashboardResponseDto dto, CancellationToken cancellationToken = default);
}
