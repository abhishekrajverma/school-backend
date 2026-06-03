using System.Text.Json;
using EduSync.SharedKernel.Results;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => ToCamelCase(g.Key), g => g.Select(x => x.ErrorMessage).ToArray());

            await WriteErrorAsync(context, StatusCodes.Status400BadRequest, Error.Validation("Validation failed.", errors));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                new Error("INTERNAL_ERROR", "An unexpected error occurred."));
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, Error error)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var payload = new
        {
            code = error.Code,
            message = error.Message,
            errors = error.FieldErrors,
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
