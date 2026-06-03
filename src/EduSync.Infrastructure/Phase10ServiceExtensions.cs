using EduSync.Infrastructure.Application.Compliance;
using EduSync.Infrastructure.Compliance;
using EduSync.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure;

public static class Phase10ServiceExtensions
{
    public static IServiceCollection AddEduSyncPhase10(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EncryptionOptions>(configuration.GetSection("Encryption"));
        services.Configure<OidcOptions>(configuration.GetSection("Oidc"));
        services.Configure<RetentionOptions>(configuration.GetSection("Retention"));
        services.AddSingleton<IFieldEncryptionService, FieldEncryptionService>();
        services.AddSingleton<IOidcTokenValidator, OidcTokenValidator>();
        services.AddHostedService<DataRetentionBackgroundService>();

        return services;
    }
}
