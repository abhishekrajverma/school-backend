using EduSync.Api.Extensions;
using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Tenancy.Application.Dtos;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class BranchEndpoints
{
    public static RouteGroupBuilder MapBranchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/branches").WithTags("Branches");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new ListBranchesQuery());
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.TenantsRead);

        group.MapGet("/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new GetBranchByIdQuery(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.TenantsRead);

        group.MapPost("/", async (CreateBranchRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateBranchCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/branches/{dto!.Id}", dto));
        }).RequirePermission(Permissions.TenantsManage);

        group.MapPatch("/{id}", async (string id, UpdateBranchRequest body, ISender sender) =>
        {
            var result = await sender.Send(new UpdateBranchCommand(id, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.TenantsManage);

        group.MapGet("/{id}/memberships", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new ListBranchMembershipsQuery(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.TenantsManage);

        group.MapPost("/{id}/memberships", async (string id, AssignBranchMembershipRequest body, ISender sender) =>
        {
            var result = await sender.Send(new AssignBranchMembershipCommand(id, body));
            return result.ToHttpResult(dto => Results.Created($"/api/branches/{id}/memberships/{dto!.UserId}", dto));
        }).RequirePermission(Permissions.TenantsManage);

        group.MapDelete("/{id}/memberships/{userId:guid}", async (string id, Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveBranchMembershipCommand(id, userId));
            return result.ToHttpResult();
        }).RequirePermission(Permissions.TenantsManage);

        return group;
    }
}
