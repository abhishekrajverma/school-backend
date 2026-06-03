using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Identity.Application.Dtos;
using EduSync.Modules.Identity.Application.Queries;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Identity;

public sealed class GetCurrentUserQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetCurrentUserQuery, Result<AuthUserDto>>
{
    public async Task<Result<AuthUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

        if (user is null)
        {
            return Result<AuthUserDto>.Failure(Error.Unauthorized("User not found."));
        }

        var membership = user.Memberships.FirstOrDefault(m => m.IsActive);
        if (membership is null)
        {
            return Result<AuthUserDto>.Failure(Error.Forbidden("No active tenant membership."));
        }

        var tenant = await db.Tenants.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == membership.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<AuthUserDto>.Failure(Error.NotFound("Tenant not found."));
        }

        return Result<AuthUserDto>.Success(
            AuthUserMapper.ToDto(user, membership, tenant.ExternalId));
    }
}
