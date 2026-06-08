using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Inventory.Application;

public sealed record InventoryItemDto(
    string Id, string Name, string Category, string Sku, int Quantity, int MinStock,
    string Unit, string Location, string Status, string LastRestocked);

public sealed record CreateInventoryItemRequest(
    string Name, string Category, string Sku, int Quantity, int MinStock,
    string Unit, string Location, string LastRestocked);

public sealed record UpdateInventoryItemRequest(
    string Name, string Category, int Quantity, int MinStock, string Location, string LastRestocked);

public sealed record ListInventoryItemsQuery(PaginationQuery Pagination, string? Category, string? Status)
    : IRequest<Result<PaginatedList<InventoryItemDto>>>;

public sealed record GetInventoryItemByIdQuery(string ExternalId) : IRequest<Result<InventoryItemDto>>;
public sealed record CreateInventoryItemCommand(CreateInventoryItemRequest Request) : IRequest<Result<InventoryItemDto>>;
public sealed record UpdateInventoryItemCommand(string ExternalId, UpdateInventoryItemRequest Request) : IRequest<Result<InventoryItemDto>>;
public sealed record DeleteInventoryItemCommand(string ExternalId) : IRequest<Result>;
