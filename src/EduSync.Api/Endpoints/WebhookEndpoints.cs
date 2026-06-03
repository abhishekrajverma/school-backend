using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Webhooks.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class WebhookEndpoints

{

    public static RouteGroupBuilder MapWebhookEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/webhooks").WithTags("Webhooks").RequirePermission(Permissions.WebhooksManage);



        group.MapGet("/", async (int? page, int? pageSize, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListWebhookSubscriptionsQuery(pagination));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        });



        group.MapPost("/", async (CreateWebhookRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateWebhookSubscriptionCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/webhooks/{dto!.Id}", dto));

        });



        group.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteWebhookSubscriptionCommand(id));

            return result.ToHttpResult(_ => Results.NoContent());

        });



        group.MapGet("/deliveries", async (int? page, int? pageSize, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListWebhookDeliveriesQuery(pagination, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        });



        return group;

    }

}


