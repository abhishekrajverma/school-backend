using EduSync.Infrastructure.MultiRegion;
using Microsoft.Extensions.Options;

namespace EduSync.Api.Endpoints;

public static class RegionEndpoints
{
    public static RouteGroupBuilder MapRegionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/region").WithTags("Multi-Region").AllowAnonymous();

        group.MapGet("/", (IRegionContext region, IOptions<MultiRegionOptions> options) =>
        {
            var cfg = options.Value;
            return Results.Ok(new
            {
                current = region.CurrentRegion ?? cfg.CurrentRegion,
                allowed = cfg.AllowedRegions,
                requireHeader = cfg.RequireRegionHeader,
            });
        });

        return group;
    }
}
