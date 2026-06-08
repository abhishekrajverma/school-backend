using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.Modules.Tenancy.Application.Queries;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Tenancy;

public sealed class GetTenantBySlugQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetTenantBySlugQuery, Result<TenantBrandingDto>>
{
    public async Task<Result<TenantBrandingDto>> Handle(GetTenantBySlugQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
        if (tenant is null)
        {
            return Result<TenantBrandingDto>.Failure(Error.NotFound("Tenant not found."));
        }

        return Result<TenantBrandingDto>.Success(new TenantBrandingDto(
            tenant.ExternalId,
            tenant.Slug,
            tenant.Name,
            tenant.LogoUrl,
            TenantStatusMapper.ToApiStatus(tenant.Status)));
    }
}
