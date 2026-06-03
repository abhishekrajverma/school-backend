using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Students.Application.Commands;

using EduSync.Modules.Students.Application.Dtos;

using EduSync.Modules.Students.Application.Queries;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class StudentEndpoints

{

    public static RouteGroupBuilder MapStudentEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/students").WithTags("Students");



        group.MapGet("/", async (

            int? page,

            int? pageSize,

            string? search,

            string? sortBy,

            string? sortOrder,

            ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, sortBy, sortOrder);

            var result = await sender.Send(new ListStudentsQuery(pagination));

            if (!result.IsSuccess)

            {

                return result.ToHttpResult();

            }



            var p = result.Value!;

            return Results.Ok(new

            {

                items = p.Items,

                page = p.Page,

                pageSize = p.PageSize,

                totalCount = p.TotalCount,

                totalPages = p.TotalPages,

            });

        }).RequirePermission(Permissions.StudentsRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetStudentByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.StudentsRead);



        group.MapPost("/", async (CreateStudentRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateStudentCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/students/{dto!.Id}", dto));

        }).RequirePermission(Permissions.StudentsWrite);



        group.MapPut("/{id}", async (string id, UpdateStudentRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateStudentCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.StudentsWrite);



        group.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteStudentCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.StudentsDelete);



        return group;

    }

}


