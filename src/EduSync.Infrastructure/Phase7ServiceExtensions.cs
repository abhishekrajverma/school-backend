using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Realtime;
using EduSync.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace EduSync.Infrastructure;

public static class Phase7ServiceExtensions
{
    public static IServiceCollection AddEduSyncPhase7(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.Configure<DatabaseOptions>(configuration.GetSection("Database"));
        services.Configure<CapacityOptions>(configuration.GetSection("Capacity"));
        services.Configure<SignalROptions>(configuration.GetSection("SignalR"));

        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();
        if (redisOptions.Enabled)
        {
            services.AddStackExchangeRedisCache(o => o.Configuration = redisOptions.ConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ITenantLookupCache, TenantLookupCache>();
        services.AddScoped<IDashboardCache, DashboardCache>();
        services.TryAddSingleton<INotificationRealtimePublisher, NoOpNotificationRealtimePublisher>();

        services.AddScoped<IReadDbContextFactory, EduSyncReadDbContextFactory>();

        return services;
    }
}
