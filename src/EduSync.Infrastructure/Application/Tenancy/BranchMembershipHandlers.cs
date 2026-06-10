using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Tenancy;

public sealed class ListBranchMembershipsQueryHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<ListBranchMembershipsQuery, Result<IReadOnlyList<BranchMembershipDto>>>
{
    public async Task<Result<IReadOnlyList<BranchMembershipDto>>> Handle(
        ListBranchMembershipsQuery request,
        CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<IReadOnlyList<BranchMembershipDto>>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.TenantId == tenant.TenantId
                     && (b.ExternalId == request.BranchExternalId || b.Code == request.BranchExternalId),
                ct);
        if (branch is null)
        {
            return Result<IReadOnlyList<BranchMembershipDto>>.Failure(Error.NotFound("Branch not found."));
        }

        var items = await db.BranchMemberships.AsNoTracking()
            .Where(m => m.BranchId == branch.Id && m.TenantId == tenant.TenantId)
            .Join(db.Users.AsNoTracking(), m => m.UserId, u => u.Id, (m, u) => new BranchMembershipDto(
                u.Id.ToString(),
                branch.ExternalId,
                m.Role,
                m.IsActive,
                m.JoinedAt))
            .ToListAsync(ct);

        return Result<IReadOnlyList<BranchMembershipDto>>.Success(items);
    }
}

public sealed class AssignBranchMembershipCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<AssignBranchMembershipCommand, Result<BranchMembershipDto>>
{
    public async Task<Result<BranchMembershipDto>> Handle(AssignBranchMembershipCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<BranchMembershipDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var branch = await db.Branches
            .FirstOrDefaultAsync(
                b => b.TenantId == tenant.TenantId
                     && (b.ExternalId == request.BranchExternalId || b.Code == request.BranchExternalId),
                ct);
        if (branch is null)
        {
            return Result<BranchMembershipDto>.Failure(Error.NotFound("Branch not found."));
        }

        var role = request.Request.Role.Trim().ToLowerInvariant();
        if (!UserRoles.All.Contains(role))
        {
            return Result<BranchMembershipDto>.Failure(Error.Validation("Invalid role."));
        }

        var normalizedEmail = request.Request.UserEmail.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);
        if (user is null)
        {
            return Result<BranchMembershipDto>.Failure(Error.NotFound("User not found."));
        }

        var hasTenantMembership = await db.TenantMemberships.AnyAsync(
            m => m.TenantId == tenant.TenantId && m.UserId == user.Id && m.IsActive,
            ct);
        if (!hasTenantMembership)
        {
            return Result<BranchMembershipDto>.Failure(Error.Conflict("User is not a member of this tenant."));
        }

        var existing = await db.BranchMemberships
            .FirstOrDefaultAsync(m => m.BranchId == branch.Id && m.UserId == user.Id, ct);

        if (existing is not null)
        {
            existing.Role = role;
            existing.IsActive = true;
        }
        else
        {
            existing = new BranchMembership
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                BranchId = branch.Id,
                UserId = user.Id,
                Role = role,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
            };
            db.BranchMemberships.Add(existing);
        }

        await db.SaveChangesAsync(ct);
        return Result<BranchMembershipDto>.Success(new BranchMembershipDto(
            user.Id.ToString(),
            branch.ExternalId,
            existing.Role,
            existing.IsActive,
            existing.JoinedAt));
    }
}

public sealed class RemoveBranchMembershipCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<RemoveBranchMembershipCommand, Result>
{
    public async Task<Result> Handle(RemoveBranchMembershipCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result.Failure(Error.Forbidden("Tenant context is required."));
        }

        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.TenantId == tenant.TenantId
                     && (b.ExternalId == request.BranchExternalId || b.Code == request.BranchExternalId),
                ct);
        if (branch is null)
        {
            return Result.Failure(Error.NotFound("Branch not found."));
        }

        var membership = await db.BranchMemberships
            .FirstOrDefaultAsync(m => m.BranchId == branch.Id && m.UserId == request.UserId, ct);
        if (membership is null)
        {
            return Result.Failure(Error.NotFound("Branch membership not found."));
        }

        membership.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
