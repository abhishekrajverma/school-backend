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



        return group;

    }

}


