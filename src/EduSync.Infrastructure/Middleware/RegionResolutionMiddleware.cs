using EduSync.Infrastructure.MultiRegion;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Middleware;

public sealed class RegionResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IRegionContext regionContext,
        IOptions<MultiRegionOptions> options)
    {
        var regionOpts = options.Value;
        var header = context.Request.Headers[HttpHeaders.Region].FirstOrDefault();
        var region = string.IsNullOrWhiteSpace(header) ? regionOpts.CurrentRegion : header.Trim();

        if (!regionOpts.AllowedRegions.Contains(region, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "INVALID_REGION",
                message = $"Region '{region}' is not supported.",
                allowed = regionOpts.AllowedRegions,
            });
            return;
        }

        if (regionOpts.RequireRegionHeader && string.IsNullOrWhiteSpace(header))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                code = "REGION_REQUIRED",
                message = $"{HttpHeaders.Region} header is required.",
            });
            return;
        }

        regionContext.Set(region);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HttpHeaders.Region] = region;
            return Task.CompletedTask;
        });

        await next(context);
    }
}
