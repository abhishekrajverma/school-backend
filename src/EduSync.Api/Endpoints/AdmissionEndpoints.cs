using EduSync.Api.Extensions;
using EduSync.Modules.Admissions.Application;
using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.Modules.Identity.Authorization;
using EduSync.SharedKernel.Pagination;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class AdmissionEndpoints
{
    public static RouteGroupBuilder MapAdmissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admissions").WithTags("Admissions");

        group.MapGet("/", async (
            int? page, int? pageSize, string? search, string? status,
            ISender sender) =>
        {
            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);
            var result = await sender.Send(new ListAdmissionsQuery(pagination, status));
            if (!result.IsSuccess) return result.ToHttpResult();
            var p = result.Value!;
            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });
        }).RequirePermission(Permissions.AdmissionsRead);

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetAdmissionByIdQuery(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.AdmissionsRead);

        group.MapPost("/", async (CreateAdmissionRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateAdmissionCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/admissions/{dto!.Id}", dto));
        }).AllowAnonymous();

        group.MapPut("/{id}", async (string id, UpdateAdmissionRequest body, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdmissionCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).AllowAnonymous();

        group.MapPost("/{id}/submit", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new SubmitAdmissionCommand(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).AllowAnonymous();

        group.MapPatch("/{id}/status", async (string id, UpdateAdmissionStatusRequest body, ISender sender) =>
        {
            var result = await sender.Send(new UpdateAdmissionStatusCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.AdmissionsManage);

        group.MapPost("/{id}/approve", async (string id, ApproveAdmissionRequest? body, ISender sender) =>
        {
            var result = await sender.Send(new ApproveAdmissionCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.AdmissionsManage);

        group.MapPost("/{id}/documents", async (string id, RegisterAdmissionDocumentRequest body, ISender sender) =>
        {
            var result = await sender.Send(new RegisterAdmissionDocumentCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).AllowAnonymous();

        return group;
    }
}
