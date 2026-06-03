using EduSync.Api.Extensions;
using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Parents.Application;
using EduSync.Modules.Parents.Application.Dtos;
using EduSync.SharedKernel.Pagination;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class ParentEndpoints
{
    public static RouteGroupBuilder MapParentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/parents").WithTags("Parents");

        group.MapGet("/", async (
            int? page, int? pageSize, string? search, string? sortBy, string? sortOrder,
            ISender sender) =>
        {
            var pagination = PaginationQuery.FromHttp(page, pageSize, search, sortBy, sortOrder);
            var result = await sender.Send(new ListParentsQuery(pagination));
            if (!result.IsSuccess) return result.ToHttpResult();
            var p = result.Value!;
            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });
        }).RequirePermission(Permissions.ParentsRead);

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetParentByIdQuery(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.ParentsRead);

        group.MapPost("/", async (CreateParentRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateParentCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/parents/{dto!.Id}", dto));
        }).RequirePermission(Permissions.ParentsWrite);

        group.MapPut("/{id}", async (string id, UpdateParentRequest body, ISender sender) =>
        {
            var result = await sender.Send(new UpdateParentCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.ParentsWrite);

        group.MapDelete("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteParentCommand(id));
            return result.ToHttpResult();
        }).RequirePermission(Permissions.ParentsDelete);

        return group;
    }
}
