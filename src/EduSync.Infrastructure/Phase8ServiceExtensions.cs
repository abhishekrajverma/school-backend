using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.MultiRegion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure;

public static class Phase8ServiceExtensions
{
    public static IServiceCollection AddEduSyncPhase8(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.Configure<MultiRegionOptions>(configuration.GetSection("MultiRegion"));
        services.AddScoped<IIntegrationEventCollector, IntegrationEventCollector>();
        services.AddScoped<IRegionContext, RegionContext>();
        services.AddSingleton<IIntegrationEventHandler, LoggingIntegrationEventHandler>();
        services.AddSingleton<IIntegrationEventHandler, AdmissionApprovedIntegrationHandler>();
        services.AddHostedService<OutboxDispatcherBackgroundService>();

        return services;
    }
}
