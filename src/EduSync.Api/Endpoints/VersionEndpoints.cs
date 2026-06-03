namespace EduSync.Api.Endpoints;

public static class VersionEndpoints
{
    public const string CurrentVersion = "1.0";
    public const string SupportedHeader = "X-Api-Version";

    public static RouteGroupBuilder MapVersionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/version").WithTags("API Version").AllowAnonymous();

        group.MapGet("/", () => Results.Ok(new
        {
            current = CurrentVersion,
            supported = new[] { "1.0" },
            routes = new[] { "/api", "/api/v1" },
            header = SupportedHeader,
        }));

        return group;
    }
}
