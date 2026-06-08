using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Inventory.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class InventoryEndpoints

{

    public static RouteGroupBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/inventory/items").WithTags("Inventory");



        group.MapGet("/", async (int? page, int? pageSize, string? search, string? category, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);

            var result = await sender.Send(new ListInventoryItemsQuery(pagination, category, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.InventoryRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetInventoryItemByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.InventoryRead);



        group.MapPost("/", async (CreateInventoryItemRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateInventoryItemCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/inventory/items/{dto!.Id}", dto));

        }).RequirePermission(Permissions.InventoryWrite);



        group.MapPut("/{id}", async (string id, UpdateInventoryItemRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateInventoryItemCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.InventoryWrite);



        group.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteInventoryItemCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.InventoryWrite);



        return group;

    }

}


