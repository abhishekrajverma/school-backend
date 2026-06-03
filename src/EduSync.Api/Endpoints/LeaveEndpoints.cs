using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Leave.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class LeaveEndpoints

{

    public static RouteGroupBuilder MapLeaveEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/leave-requests").WithTags("Leave");



        group.MapGet("/", async (int? page, int? pageSize, string? status, string? employeeId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListLeaveRequestsQuery(pagination, status, employeeId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.LeaveRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetLeaveByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.LeaveRead);



        group.MapPost("/", async (CreateLeaveRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateLeaveCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/leave-requests/{dto!.Id}", dto));

        }).RequirePermission(Permissions.LeaveWrite);



        group.MapPost("/{id}/approve", async (string id, ApproveLeaveBody? body, ISender sender) =>

        {

            var result = await sender.Send(new ApproveLeaveCommand(id, body?.ApprovedBy));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.LeaveApprove);



        group.MapPost("/{id}/reject", async (string id, ApproveLeaveBody? body, ISender sender) =>

        {

            var result = await sender.Send(new RejectLeaveCommand(id, body?.ApprovedBy));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.LeaveApprove);



        return group;

    }



    private sealed record ApproveLeaveBody(string? ApprovedBy);

}


