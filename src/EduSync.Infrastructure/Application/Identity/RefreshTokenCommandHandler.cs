using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Application.Commands;
using EduSync.Modules.Identity.Application.Dtos;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Identity;

public sealed class RefreshTokenCommandHandler(
    EduSyncDbContext db,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = jwtTokenService.HashToken(request.RefreshToken);
        var existing = await db.RefreshTokens
            .Include(r => r.User)
            .ThenInclude(u => u.Memberships)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            return Result<LoginResponse>.Failure(Error.Unauthorized("Invalid refresh token."));
        }

        existing.RevokedAt = DateTime.UtcNow;
        var refreshPlain = jwtTokenService.GenerateRefreshToken();
        var replacement = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existing.UserId,
            TokenHash = jwtTokenService.HashToken(refreshPlain),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            ReplacedByTokenHash = hash,
        };
        db.RefreshTokens.Add(replacement);
        await db.SaveChangesAsync(cancellationToken);

        if (string.Equals(existing.User.Role, UserRoles.Company, StringComparison.OrdinalIgnoreCase))
        {
            var (companyToken, companyExpires) = jwtTokenService.CreateAccessToken(
                existing.User, Guid.Empty, "platform", UserRoles.Company);
            return Result<LoginResponse>.Success(new LoginResponse(
                companyToken,
                refreshPlain,
                companyExpires,
                AuthUserMapper.ToCompanyDto(existing.User)));
        }

        var membership = existing.User.Memberships.FirstOrDefault(m => m.IsActive);
        if (membership is null)
        {
            return Result<LoginResponse>.Failure(Error.Forbidden("No active tenant membership."));
        }

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == membership.TenantId, cancellationToken);
        var tenantGuard = TenantLoginGuard.ValidateForLogin(tenant);
        if (tenantGuard is not null)
        {
            return Result<LoginResponse>.Failure(tenantGuard.Error!);
        }

        var (accessToken, expiresIn) = jwtTokenService.CreateAccessToken(
            existing.User, tenant.Id, tenant.ExternalId, membership.Role);
        return Result<LoginResponse>.Success(new LoginResponse(
            accessToken,
            refreshPlain,
            expiresIn,
            AuthUserMapper.ToDto(existing.User, membership, tenant.ExternalId)));
    }
}
