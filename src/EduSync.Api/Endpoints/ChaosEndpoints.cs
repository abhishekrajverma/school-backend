using EduSync.Api.Extensions;

using EduSync.Infrastructure.Chaos;

using EduSync.Modules.Identity.Authorization;

using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Options;



namespace EduSync.Api.Endpoints;



public static class ChaosEndpoints

{

    public static RouteGroupBuilder MapChaosEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/chaos").WithTags("Chaos Engineering").RequirePermission(Permissions.ChaosRead);



        group.MapGet("/config", (IOptions<ChaosOptions> options, IWebHostEnvironment env) =>

        {

            var cfg = options.Value;

            return Results.Ok(new

            {

                cfg.Enabled,

                cfg.FailureRate,

                cfg.MaxLatencyMs,

                cfg.AllowInProduction,

                environment = env.EnvironmentName,

                note = "Inject random latency/failures when enabled (development by default).",

            });

        });



        return group;

    }

}


