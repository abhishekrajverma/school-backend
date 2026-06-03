using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure.Authorization;

public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddEduSyncAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IPermissionService, PermissionService>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(permission, policy =>
                    policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }
}
