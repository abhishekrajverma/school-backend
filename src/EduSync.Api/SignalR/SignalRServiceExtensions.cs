using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace EduSync.Api.SignalR;

public static class SignalRServiceExtensions
{
    public static IServiceCollection AddEduSyncSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var signalROptions = configuration.GetSection("SignalR").Get<SignalROptions>() ?? new SignalROptions();
        if (!signalROptions.Enabled)
        {
            return services;
        }

        var signalRBuilder = services.AddSignalR();
        var redisOptions = configuration.GetSection("Redis").Get<RedisOptions>() ?? new RedisOptions();
        if (redisOptions.Enabled)
        {
            signalRBuilder.AddStackExchangeRedis(redisOptions.ConnectionString, options =>
            {
                options.Configuration.ChannelPrefix = RedisChannel.Literal("edusync");
            });
        }

        services.RemoveAll<INotificationRealtimePublisher>();
        services.AddSingleton<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();
        return services;
    }

    public static WebApplication MapEduSyncSignalR(this WebApplication app, IConfiguration configuration)
    {
        if (!configuration.GetValue("SignalR:Enabled", true))
        {
            return app;
        }

        app.MapHub<NotificationsHub>(NotificationsHub.HubPath);
        return app;
    }
}
