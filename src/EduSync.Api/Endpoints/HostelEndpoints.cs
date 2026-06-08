using EduSync.Api.Extensions;

using EduSync.Modules.Hostel.Application;

using EduSync.Modules.Identity.Authorization;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class HostelEndpoints

{

    public static void MapHostelEndpoints(this IEndpointRouteBuilder app)

    {

        var rooms = app.MapGroup("/hostel/rooms").WithTags("Hostel");



        rooms.MapGet("/", async (int? page, int? pageSize, string? block, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListHostelRoomsQuery(pagination, block, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.HostelRead);



        rooms.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetHostelRoomByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.HostelRead);



        rooms.MapPost("/", async (CreateHostelRoomRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateHostelRoomCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/hostel/rooms/{dto!.Id}", dto));

        }).RequirePermission(Permissions.HostelWrite);



        rooms.MapPut("/{id}", async (string id, UpdateHostelRoomRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateHostelRoomCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.HostelWrite);



        rooms.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteHostelRoomCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.HostelWrite);



        var allocations = app.MapGroup("/hostel/allocations").WithTags("Hostel");



        allocations.MapGet("/", async (int? page, int? pageSize, string? roomId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListHostelAllocationsQuery(pagination, roomId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.HostelRead);



        allocations.MapPost("/", async (CreateAllocationRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateHostelAllocationCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/hostel/allocations/{dto!.Id}", dto));

        }).RequirePermission(Permissions.HostelWrite);

    }

}


