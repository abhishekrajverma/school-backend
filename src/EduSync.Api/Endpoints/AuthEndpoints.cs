using System.Security.Claims;
using EduSync.Api.Extensions;
using EduSync.Modules.Identity.Application.Commands;
using EduSync.Modules.Identity.Application.Dtos;
using EduSync.Modules.Identity.Application.Queries;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapGet("/oidc/config", async (ISender sender) =>
        {
            var config = await sender.Send(new GetOidcConfigQuery());
            return Results.Ok(config);
        }).AllowAnonymous();

        group.MapPost("/oidc/login", async (OidcLoginRequest body, ISender sender) =>
        {
            if (string.IsNullOrWhiteSpace(body.IdToken))
            {
                return Results.Json(
                    new { code = "VALIDATION_ERROR", message = "idToken is required." },
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await sender.Send(new OidcLoginCommand(body.IdToken));
            return result.ToHttpResult();
        }).AllowAnonymous();

        group.MapPost("/login", async (LoginRequest body, ISender sender) =>
        {
            var result = await sender.Send(new LoginCommand(body.Email, body.Password));
            return result.ToHttpResult();
        }).AllowAnonymous();

        group.MapPost("/refresh", async (RefreshRequest body, ISender sender) =>
        {
            if (string.IsNullOrWhiteSpace(body.RefreshToken))
            {
                return Results.Json(
                    new { code = "VALIDATION_ERROR", message = "Refresh token is required." },
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await sender.Send(new RefreshTokenCommand(body.RefreshToken));
            return result.ToHttpResult();
        }).AllowAnonymous();

        group.MapGet("/me", async (ClaimsPrincipal user, ISender sender) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Json(
                    new { code = "AUTH_FAILED", message = "Not authenticated." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await sender.Send(new GetCurrentUserQuery(userId.Value));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequireAuthorization();

        group.MapPost("/logout", async (RefreshRequest body, ClaimsPrincipal user, ISender sender) =>
        {
            var userId = GetUserId(user);
            if (userId is null)
            {
                return Results.Json(
                    new { code = "AUTH_FAILED", message = "Not authenticated." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await sender.Send(new LogoutCommand(userId.Value, body.RefreshToken));
            return result.ToHttpResult();
        }).RequireAuthorization();

        return group;
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}
