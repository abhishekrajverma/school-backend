using EduSync.Api.Extensions;

using EduSync.Modules.Academics.Application;

using EduSync.Modules.Academics.Application.Dtos;

using EduSync.Modules.Identity.Authorization;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class AcademicsEndpoints

{

    public static RouteGroupBuilder MapAcademicsEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/academics").WithTags("Academics");



        group.MapGet("/classes", async (ISender sender) =>

        {

            var result = await sender.Send(new ListClassesQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AcademicsRead);



        group.MapPost("/classes", async (CreateClassRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateClassCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/academics/classes/{dto!.Id}", dto));

        }).RequirePermission(Permissions.AcademicsWrite);



        group.MapGet("/subjects", async (string? className, ISender sender) =>

        {

            var result = await sender.Send(new ListSubjectsQuery(className));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AcademicsRead);



        group.MapPost("/subjects", async (CreateSubjectRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateSubjectCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/academics/subjects/{dto!.Id}", dto));

        }).RequirePermission(Permissions.AcademicsWrite);



        group.MapPut("/classes/{id}", async (string id, UpdateClassRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateClassCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AcademicsWrite);



        group.MapDelete("/classes/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteClassCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.AcademicsWrite);



        group.MapPut("/subjects/{id}", async (string id, UpdateSubjectRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateSubjectCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AcademicsWrite);



        group.MapDelete("/subjects/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteSubjectCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.AcademicsWrite);



        return group;

    }

}


