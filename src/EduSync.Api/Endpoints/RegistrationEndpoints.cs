using EduSync.Api.Extensions;
using EduSync.Modules.Admissions.Application;
using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.Modules.Identity.Authorization;
using EduSync.SharedKernel.Pagination;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class RegistrationEndpoints
{
    public static RouteGroupBuilder MapRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/registrations").WithTags("Registrations");

        group.MapGet("/", async (int? page, int? pageSize, string? search, string? status, ISender sender) =>
        {
            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);
            var result = await sender.Send(new ListRegistrationsQuery(pagination, status));
            if (!result.IsSuccess) return result.ToHttpResult();
            var p = result.Value!;
            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });
        }).RequirePermission(Permissions.AdmissionsRead);

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetRegistrationByIdQuery(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.AdmissionsRead);

        group.MapPost("/", async (CreateRegistrationRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateRegistrationCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/registrations/{dto!.Id}", dto));
        }).AllowAnonymous();

        group.MapPut("/{id}", async (string id, UpdateRegistrationRequest body, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRegistrationCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).AllowAnonymous();

        group.MapPost("/{id}/submit", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new SubmitRegistrationCommand(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).AllowAnonymous();

        group.MapPost("/{id}/convert-to-admission", async (string id, ConvertRegistrationToAdmissionRequest? body, ISender sender) =>
        {
            var result = await sender.Send(new ConvertRegistrationToAdmissionCommand(id, body));
            return result.ToHttpResult(dto => Results.Created($"/api/admissions/{dto!.Id}", dto));
        }).RequirePermission(Permissions.AdmissionsManage);

        return group;
    }
}
