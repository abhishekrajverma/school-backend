using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Tenancy.Application.Commands;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduSync.Infrastructure.Application.Tenancy;

public sealed class ProvisionTenantCommandHandler(
    EduSyncDbContext db,
    IPasswordHasher passwordHasher,
    IConfiguration configuration) : IRequestHandler<ProvisionTenantCommand, Result<ProvisionTenantResponse>>
{
    public async Task<Result<ProvisionTenantResponse>> Handle(
        ProvisionTenantCommand request,
        CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await db.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            return Result<ProvisionTenantResponse>.Failure(Error.Conflict("Slug is already in use."));
        }

        var normalizedEmail = request.AdminEmail.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return Result<ProvisionTenantResponse>.Failure(Error.Conflict("Admin email is already registered."));
        }

        var tenantId = Guid.NewGuid();
        var externalId = slug.Replace('-', '_') + "_" + tenantId.ToString("N")[..8];
        var tenant = new Tenant
        {
            Id = tenantId,
            ExternalId = externalId,
            Slug = slug,
            Name = request.SchoolName.Trim(),
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Subscription = new TenantSubscription
            {
                TenantId = tenantId,
                PlanId = request.PlanId,
                SeatLimit = ResolveSeatLimit(request.PlanId),
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                FeatureFlagsJson = "{}",
            },
        };

        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = $"{DateTime.UtcNow.Year}-{(DateTime.UtcNow.Year + 1) % 100:D2}",
            StartDate = new DateOnly(DateTime.UtcNow.Year, 4, 1),
            EndDate = new DateOnly(DateTime.UtcNow.Year + 1, 3, 31),
            IsCurrent = true,
        };

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            ExternalId = "admin",
            Email = request.AdminEmail.Trim(),
            NormalizedEmail = normalizedEmail,
            Name = request.AdminName.Trim(),
            PasswordHash = passwordHasher.Hash(request.AdminPassword),
            Role = UserRoles.Admin,
            IsActive = true,
            Memberships =
            [
                new TenantMembership
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    UserId = default,
                    Role = UserRoles.Admin,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                },
            ],
        };
        adminUser.Memberships.First().UserId = adminUser.Id;

        db.Tenants.Add(tenant);
        db.AcademicYears.Add(academicYear);
        db.Users.Add(adminUser);
        await db.SaveChangesAsync(cancellationToken);

        var appUrl = configuration["App:BaseUrl"] ?? "http://localhost:3000";
        var portalUrl = $"{appUrl.TrimEnd('/')}/school/{slug}/login";

        return Result<ProvisionTenantResponse>.Success(new ProvisionTenantResponse(externalId, slug, portalUrl));
    }

    private static int ResolveSeatLimit(string planId) => planId.ToLowerInvariant() switch
    {
        "enterprise" => 500,
        "professional" => 150,
        _ => 50,
    };
}
