using EduSync.Api.Extensions;

using EduSync.Modules.Events.Application;

using EduSync.Modules.Identity.Authorization;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class EventEndpoints

{

    public static RouteGroupBuilder MapEventEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/events").WithTags("Integration Events").RequirePermission(Permissions.EventsRead);



        group.MapGet("/outbox", async (int? page, int? pageSize, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListOutboxMessagesQuery(pagination, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        });



        return group;

    }

}


