using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Application.Commands;
using EduSync.Modules.Identity.Application.Dtos;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Identity;

public sealed class LoginCommandHandler(
    EduSyncDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized && u.IsActive, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure(Error.Unauthorized("Invalid email or password."));
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
