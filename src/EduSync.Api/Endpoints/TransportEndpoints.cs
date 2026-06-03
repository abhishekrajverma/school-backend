using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Transport.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class TransportEndpoints

{

    public static void MapTransportEndpoints(this IEndpointRouteBuilder app)

    {

        var vehicles = app.MapGroup("/transport/vehicles").WithTags("Transport");



        vehicles.MapGet("/", async (int? page, int? pageSize, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListVehiclesQuery(pagination, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.TransportRead);



        vehicles.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetVehicleByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TransportRead);



        vehicles.MapPost("/", async (CreateVehicleRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateVehicleCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/transport/vehicles/{dto!.Id}", dto));

        }).RequirePermission(Permissions.TransportWrite);



        var routes = app.MapGroup("/transport/routes").WithTags("Transport");



        routes.MapGet("/", async (int? page, int? pageSize, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListRoutesQuery(pagination, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.TransportRead);



        routes.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetRouteByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TransportRead);



        routes.MapPost("/", async (CreateRouteRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateRouteCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/transport/routes/{dto!.Id}", dto));

        }).RequirePermission(Permissions.TransportWrite);



        var assignments = app.MapGroup("/transport/assignments").WithTags("Transport");



        assignments.MapGet("/", async (int? page, int? pageSize, string? routeId, string? studentId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListTransportAssignmentsQuery(pagination, routeId, studentId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.TransportRead);



        assignments.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetTransportAssignmentByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TransportRead);



        assignments.MapPost("/", async (CreateTransportAssignmentRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateTransportAssignmentCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/transport/assignments/{dto!.Id}", dto));

        }).RequirePermission(Permissions.TransportWrite);

    }

}


