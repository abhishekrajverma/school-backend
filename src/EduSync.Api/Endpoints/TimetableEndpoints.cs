using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Timetable.Application;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class TimetableEndpoints

{

    public static RouteGroupBuilder MapTimetableEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/timetable").WithTags("Timetable");



        group.MapGet("/", async (string? className, string? day, ISender sender) =>

        {

            var result = await sender.Send(new ListTimetableQuery(className, day));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TimetableRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetTimetableByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TimetableRead);



        group.MapPut("/", async (UpsertTimetableRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpsertTimetableCommand(body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.TimetableWrite);



        return group;

    }

}


