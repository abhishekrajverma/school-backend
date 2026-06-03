using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Inventory.Application;
using EduSync.Modules.Inventory.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Inventory;

internal static class InventoryMapping
{
    public static InventoryItemDto ToDto(InventoryItem i) => new(
        i.ExternalId, i.Name, i.Category, i.Sku, i.Quantity, i.MinStock,
        i.Unit, i.Location, i.Status, i.LastRestocked.ToString("yyyy-MM-dd"));

    public static void RefreshStatus(InventoryItem i) =>
        i.Status = i.Quantity <= i.MinStock ? "low-stock" : "in-stock";
}

public sealed class ListInventoryItemsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListInventoryItemsQuery, Result<PaginatedList<InventoryItemDto>>>
{
    public async Task<Result<PaginatedList<InventoryItemDto>>> Handle(ListInventoryItemsQuery request, CancellationToken ct)
    {
        var query = db.InventoryItems.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category == request.Category);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        query = query.OrderBy(x => x.Name);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(InventoryMapping.ToDto).ToList();
        return Result<PaginatedList<InventoryItemDto>>.Success(
            PaginatedList<InventoryItemDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetInventoryItemByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetInventoryItemByIdQuery, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(GetInventoryItemByIdQuery request, CancellationToken ct)
    {
        var i = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return i is null ? Result<InventoryItemDto>.Failure(Error.NotFound("Item not found."))
            : Result<InventoryItemDto>.Success(InventoryMapping.ToDto(i));
    }
}

public sealed class CreateInventoryItemCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateInventoryItemCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(CreateInventoryItemCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<InventoryItemDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.LastRestocked, out var restocked))
            return Result<InventoryItemDto>.Failure(Error.Validation("Invalid restock date."));
        var item = new InventoryItem
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            Name = b.Name, Category = b.Category, Sku = b.Sku,
            Quantity = b.Quantity, MinStock = b.MinStock, Unit = b.Unit,
            Location = b.Location, LastRestocked = restocked,
        };
        InventoryMapping.RefreshStatus(item);
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync(ct);
        return Result<InventoryItemDto>.Success(InventoryMapping.ToDto(item));
    }
}

public sealed class UpdateInventoryItemCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateInventoryItemCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(UpdateInventoryItemCommand request, CancellationToken ct)
    {
        var item = await db.InventoryItems.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (item is null) return Result<InventoryItemDto>.Failure(Error.NotFound("Item not found."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.LastRestocked, out var restocked))
            return Result<InventoryItemDto>.Failure(Error.Validation("Invalid restock date."));
        item.Name = b.Name; item.Category = b.Category; item.Quantity = b.Quantity;
        item.MinStock = b.MinStock; item.Location = b.Location; item.LastRestocked = restocked;
        InventoryMapping.RefreshStatus(item);
        await db.SaveChangesAsync(ct);
        return Result<InventoryItemDto>.Success(InventoryMapping.ToDto(item));
    }
}
