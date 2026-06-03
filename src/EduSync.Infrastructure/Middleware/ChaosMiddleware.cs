using EduSync.Infrastructure.Chaos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Middleware;

public sealed class ChaosMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/health",
        "/api/chaos",
        "/swagger",
        "/hangfire",
        "/hubs",
        "/gateway/health",
    };

    public async Task InvokeAsync(
        HttpContext context,
        IOptions<ChaosOptions> options,
        IWebHostEnvironment environment)
    {
        var chaos = options.Value;
        if (!chaos.Enabled)
        {
            await next(context);
            return;
        }

        if (!chaos.AllowInProduction && !environment.IsDevelopment())
        {
            await next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        if (chaos.MaxLatencyMs > 0)
        {
            var delay = Random.Shared.Next(0, chaos.MaxLatencyMs);
            if (delay > 0)
            {
                await Task.Delay(delay, context.RequestAborted);
            }
        }

        if (chaos.FailureRate > 0 && Random.Shared.NextDouble() < chaos.FailureRate)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers["X-Chaos-Injected"] = "true";
            await context.Response.WriteAsJsonAsync(new
            {
                code = "CHAOS_FAILURE",
                message = "Simulated failure (chaos engineering).",
            });
            return;
        }

        await next(context);
    }
}
