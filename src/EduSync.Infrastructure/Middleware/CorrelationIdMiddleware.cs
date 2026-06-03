using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;

namespace EduSync.Infrastructure.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HttpHeaders.CorrelationId].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.Items[HttpHeaders.CorrelationId] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HttpHeaders.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        context.TraceIdentifier = correlationId;
        await next(context);
    }
}
