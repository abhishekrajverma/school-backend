using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Notifications.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class NotificationEndpoints

{

    public static RouteGroupBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/notifications").WithTags("Notifications");



        group.MapGet("/", async (int? page, int? pageSize, string? targetAudience, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListNotificationsQuery(pagination, targetAudience));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.NotificationsRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetNotificationByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.NotificationsRead);



        group.MapPost("/", async (CreateNotificationRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateNotificationCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/notifications/{dto!.Id}", dto));

        }).RequirePermission(Permissions.NotificationsWrite);



        group.MapPost("/{id}/read", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new MarkNotificationReadCommand(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.NotificationsRead);



        return group;

    }

}


