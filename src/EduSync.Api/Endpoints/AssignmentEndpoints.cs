using EduSync.Api.Extensions;
using EduSync.Modules.Assignments.Application;
using EduSync.Modules.Identity.Authorization;
using EduSync.SharedKernel.Pagination;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class AssignmentEndpoints
{
    public static RouteGroupBuilder MapAssignmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/assignments").WithTags("Assignments");

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? className, ISender sender) =>
        {
            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);
            var result = await sender.Send(new ListAssignmentsQuery(pagination, className));
            if (!result.IsSuccess) return result.ToHttpResult();
            var p = result.Value!;
            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });
        }).RequirePermission(Permissions.AssignmentsRead);

        group.MapPost("/", async (CreateAssignmentRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateAssignmentCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/assignments/{dto!.Id}", dto));
        }).RequirePermission(Permissions.AssignmentsWrite);

        return group;
    }
}
