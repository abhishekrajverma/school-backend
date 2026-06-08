using EduSync.Api.Extensions;
using EduSync.Modules.Company.Application;
using EduSync.Modules.Identity.Authorization;
using EduSync.SharedKernel.Pagination;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class EnquiryEndpoints
{
    public static RouteGroupBuilder MapEnquiryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/enquiries").WithTags("Enquiries");

        group.MapPost("/", async (CreateEnquiryRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateEnquiryCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/enquiries/{dto!.Id}", dto));
        }).AllowAnonymous();

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? status, ISender sender) =>
        {
            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);
            var result = await sender.Send(new ListEnquiriesQuery(pagination, status));
            if (!result.IsSuccess) return result.ToHttpResult();
            var p = result.Value!;
            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });
        }).RequirePermission(Permissions.EnquiriesRead);

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetEnquiryByIdQuery(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.EnquiriesRead);

        group.MapPatch("/{id}", async (string id, UpdateEnquiryRequest body, ISender sender) =>
        {
            var result = await sender.Send(new UpdateEnquiryCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.EnquiriesManage);

        return group;
    }
}
