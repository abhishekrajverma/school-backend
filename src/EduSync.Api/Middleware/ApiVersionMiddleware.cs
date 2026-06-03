using EduSync.Api.Endpoints;

namespace EduSync.Api.Middleware;

public sealed class ApiVersionMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SupportedVersions = new(StringComparer.OrdinalIgnoreCase) { "1.0", "1" };

    public async Task InvokeAsync(HttpContext context)
    {
        var versionHeader = context.Request.Headers[VersionEndpoints.SupportedHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(versionHeader) && !SupportedVersions.Contains(versionHeader.Trim()))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "UNSUPPORTED_API_VERSION",
                message = $"API version '{versionHeader}' is not supported.",
                supported = SupportedVersions,
            });
            return;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[VersionEndpoints.SupportedHeader] = VersionEndpoints.CurrentVersion;
            return Task.CompletedTask;
        });

        await next(context);
    }
}
