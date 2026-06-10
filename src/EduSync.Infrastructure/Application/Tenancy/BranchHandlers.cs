using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BranchEntity = EduSync.Modules.Tenancy.Domain.Branch;

namespace EduSync.Infrastructure.Application.Tenancy;

internal static class BranchMapping
{
    public static BranchDto ToDto(BranchEntity b) => new(
        b.ExternalId,
        b.Code,
        b.Name,
        b.Address,
        b.IsHeadOffice,
        b.IsActive);
}

public sealed class ListBranchesQueryHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<ListBranchesQuery, Result<IReadOnlyList<BranchDto>>>
{
    public async Task<Result<IReadOnlyList<BranchDto>>> Handle(ListBranchesQuery request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<IReadOnlyList<BranchDto>>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var items = await db.Branches.AsNoTracking()
            .Where(b => b.TenantId == tenant.TenantId && b.IsActive)
            .OrderByDescending(b => b.IsHeadOffice)
            .ThenBy(b => b.Name)
            .ToListAsync(ct);

        return Result<IReadOnlyList<BranchDto>>.Success(items.Select(BranchMapping.ToDto).ToList());
    }
}

public sealed class GetBranchByIdQueryHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
{
    public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<BranchDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var branch = await db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.TenantId == tenant.TenantId
                     && (b.ExternalId == request.ExternalId || b.Code == request.ExternalId),
                ct);

        return branch is null
            ? Result<BranchDto>.Failure(Error.NotFound("Branch not found."))
            : Result<BranchDto>.Success(BranchMapping.ToDto(branch));
    }
}

public sealed class CreateBranchCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<BranchDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var code = request.Request.Code.Trim().ToUpperInvariant();
        if (await db.Branches.AnyAsync(b => b.TenantId == tenant.TenantId && b.Code == code, ct))
        {
            return Result<BranchDto>.Failure(Error.Conflict("Branch code already exists."));
        }

        if (request.Request.IsHeadOffice)
        {
            var existing = await db.Branches
                .Where(b => b.TenantId == tenant.TenantId && b.IsHeadOffice)
                .ToListAsync(ct);
            foreach (var b in existing)
            {
                b.IsHeadOffice = false;
            }
        }

        var branch = new BranchEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            Code = code,
            Name = request.Request.Name.Trim(),
            Address = request.Request.Address?.Trim(),
            IsHeadOffice = request.Request.IsHeadOffice,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        db.Branches.Add(branch);
        await db.SaveChangesAsync(ct);
        return Result<BranchDto>.Success(BranchMapping.ToDto(branch));
    }
}

public sealed class UpdateBranchCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
{
    public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken ct)
    {
        var branch = await db.Branches.FirstOrDefaultAsync(
            b => b.ExternalId == request.ExternalId || b.Code == request.ExternalId, ct);
        if (branch is null)
        {
            return Result<BranchDto>.Failure(Error.NotFound("Branch not found."));
        }

        var body = request.Request;
        if (body.Name is not null) branch.Name = body.Name.Trim();
        if (body.Address is not null) branch.Address = body.Address.Trim();
        if (body.IsActive.HasValue) branch.IsActive = body.IsActive.Value;

        await db.SaveChangesAsync(ct);
        return Result<BranchDto>.Success(BranchMapping.ToDto(branch));
    }
}
