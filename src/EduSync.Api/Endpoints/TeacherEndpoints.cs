using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Staff.Application;

using EduSync.Modules.Staff.Application.Dtos;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class TeacherEndpoints

{

    public static RouteGroupBuilder MapTeacherEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/teachers").WithTags("Teachers");



        group.MapGet("/", async (

            int? page, int? pageSize, string? search, string? sortBy, string? sortOrder,

            ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, sortBy, sortOrder);

            var result = await sender.Send(new ListTeachersQuery(pagination));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.TeachersRead);

        group.MapGet("/assignments", async (string? teacherId, Guid? academicYearId, ISender sender) =>
        {
            var result = await sender.Send(new ListTeacherAssignmentsQuery(teacherId, academicYearId));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.TeachersRead);

        group.MapPost("/assignments", async (CreateTeacherAssignmentRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateTeacherAssignmentCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/teachers/assignments/{dto!.Id}", dto));
        }).RequirePermission(Permissions.TeachersWrite);

        group.MapDelete("/assignments/{id}", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new DeactivateTeacherAssignmentCommand(id));
            return result.ToHttpResult();
        }).RequirePermission(Permissions.TeachersWrite);

        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetTeacherByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TeachersRead);



        group.MapPost("/", async (CreateTeacherRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateTeacherCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/teachers/{dto!.Id}", dto));

        }).RequirePermission(Permissions.TeachersWrite);



        group.MapPut("/{id}", async (string id, UpdateTeacherRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateTeacherCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TeachersWrite);



        group.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteTeacherCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.TeachersDelete);

        return group;

    }

}


