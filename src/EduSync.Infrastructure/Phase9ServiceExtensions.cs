using EduSync.Infrastructure.Audit;
using EduSync.Infrastructure.Chaos;
using EduSync.Infrastructure.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure;

public static class Phase9ServiceExtensions
{
    public static IServiceCollection AddEduSyncPhase9(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuditOptions>(configuration.GetSection("Audit"));
        services.Configure<ChaosOptions>(configuration.GetSection("Chaos"));
        services.AddHttpClient("webhooks", client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<IIntegrationEventHandler, WebhookIntegrationEventHandler>();

        return services;
    }
}
