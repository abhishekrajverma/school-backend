using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.Modules.Tenancy.Application.Queries;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Tenancy;

public sealed class GetCurrentTenantQueryHandler(IReadDbContextFactory dbFactory)
    : IRequestHandler<GetCurrentTenantQuery, Result<CurrentTenantDto>>
{
    public async Task<Result<CurrentTenantDto>> Handle(GetCurrentTenantQuery request, CancellationToken cancellationToken)
    {
        await using var db = dbFactory.CreateDbContext();
        var tenant = await db.Tenants
            .AsNoTracking()
            .Include(t => t.Subscription)
            .FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return Result<CurrentTenantDto>.Failure(Error.NotFound("Tenant not found."));
        }

        return Result<CurrentTenantDto>.Success(new CurrentTenantDto(
            tenant.ExternalId,
            tenant.Slug,
            tenant.Name,
            tenant.LogoUrl,
            tenant.Status.ToString().ToLowerInvariant(),
            tenant.Subscription?.PlanId ?? "starter",
            tenant.Subscription?.SeatLimit ?? 50,
            tenant.Subscription?.ExpiresAt));
    }
}
