using EduSync.Modules.Dashboard.Application;

namespace EduSync.Infrastructure.Caching;

public interface IDashboardCache
{
    Task<DashboardResponseDto?> GetAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task SetAsync(Guid tenantId, DashboardResponseDto dto, CancellationToken cancellationToken = default);
}
