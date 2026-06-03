using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Security;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Application.Commands;
using EduSync.Modules.Identity.Application.Dtos;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EduSync.Infrastructure.Application.Identity;

public sealed class GetOidcConfigQueryHandler(IOptions<OidcOptions> options)
    : IRequestHandler<GetOidcConfigQuery, OidcConfigDto>
{
    public Task<OidcConfigDto> Handle(GetOidcConfigQuery request, CancellationToken cancellationToken)
    {
        var cfg = options.Value;
        return Task.FromResult(new OidcConfigDto(cfg.Enabled, cfg.Authority, cfg.ClientId, cfg.Scopes));
    }
}

public sealed class OidcLoginCommandHandler(
    EduSyncDbContext db,
    IOidcTokenValidator oidcValidator,
    IJwtTokenService jwtTokenService,
    IOptions<OidcOptions> options) : IRequestHandler<OidcLoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(OidcLoginCommand request, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return Result<LoginResponse>.Failure(Error.Validation("OIDC login is not enabled."));
        }

        var principal = await oidcValidator.ValidateIdTokenAsync(request.IdToken, cancellationToken);
        if (principal is null)
        {
            return Result<LoginResponse>.Failure(Error.Unauthorized("Invalid OIDC token."));
        }

        var email = principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email")
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<LoginResponse>.Failure(Error.Validation("OIDC token must include email claim."));
        }

        var normalized = email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized && u.IsActive, cancellationToken);

        if (user is null)
        {
            return Result<LoginResponse>.Failure(Error.NotFound(
                "No local user linked to this identity. Provision the user first or use password login."));
        }

        var membership = user.Memberships.FirstOrDefault(m => m.IsActive);
        if (membership is null)
        {
            return Result<LoginResponse>.Failure(Error.Forbidden("No active tenant membership."));
        }

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == membership.TenantId, cancellationToken);
        if (tenant is null || tenant.Status == TenantStatus.Suspended)
        {
            return Result<LoginResponse>.Failure(Error.Forbidden("Tenant is not available."));
        }

        var (accessToken, expiresIn) = jwtTokenService.CreateAccessToken(
            user, tenant.Id, tenant.ExternalId, membership.Role);
        var refreshPlain = jwtTokenService.GenerateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = jwtTokenService.HashToken(refreshPlain),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
        });
        await db.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshPlain,
            expiresIn,
            AuthUserMapper.ToDto(user, membership, tenant.ExternalId)));
    }
}
