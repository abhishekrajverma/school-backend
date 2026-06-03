using EduSync.Api.Extensions;

using EduSync.Modules.Audit.Application;

using EduSync.Modules.Identity.Authorization;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class AuditEndpoints

{

    public static RouteGroupBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/audit").WithTags("Audit").RequirePermission(Permissions.AuditRead);



        group.MapGet("/logs", async (int? page, int? pageSize, string? action, string? path, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListAuditLogsQuery(pagination, action, path));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        });



        return group;

    }

}


