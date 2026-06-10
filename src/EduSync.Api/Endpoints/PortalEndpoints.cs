using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Portals.Application;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class PortalEndpoints

{

    public static void MapPortalEndpoints(this IEndpointRouteBuilder app)

    {

        var student = app.MapGroup("/students/me").WithTags("Student Portal");

        student.MapGet("/", async (ISender sender) =>

        {

            var result = await sender.Send(new GetStudentPortalProfileQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalStudent);

        student.MapGet("/fees", async (ISender sender) =>

        {

            var result = await sender.Send(new GetStudentPortalFeesQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalStudent);

        student.MapGet("/attendance", async (ISender sender) =>

        {

            var result = await sender.Send(new GetStudentPortalAttendanceQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalStudent);

        student.MapGet("/exams", async (ISender sender) =>

        {

            var result = await sender.Send(new GetStudentPortalExamsQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalStudent);

        student.MapGet("/assignments", async (ISender sender) =>
        {
            var result = await sender.Send(new GetStudentPortalAssignmentsQuery());
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.PortalStudent);

        student.MapPost("/assignments/{assignmentId}/submit", async (string assignmentId, EduSync.Modules.Assignments.Application.SubmitAssignmentRequest body, ISender sender) =>
        {
            var result = await sender.Send(new EduSync.Modules.Assignments.Application.SubmitStudentAssignmentCommand(assignmentId, body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.PortalStudent);

        student.MapGet("/timetable", async (ISender sender) =>

        {

            var result = await sender.Send(new GetStudentPortalTimetableQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalStudent);

        student.MapGet("/library/issues", async (ISender sender) =>

        {

            var result = await sender.Send(new GetStudentPortalLibraryQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalStudent);



        var teacher = app.MapGroup("/teachers/me").WithTags("Teacher Portal");

        teacher.MapGet("/", async (ISender sender) =>

        {

            var result = await sender.Send(new GetTeacherPortalProfileQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalTeacher);

        teacher.MapGet("/leaves", async (ISender sender) =>

        {

            var result = await sender.Send(new GetTeacherPortalLeavesQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalTeacher);

        teacher.MapGet("/payroll", async (ISender sender) =>

        {

            var result = await sender.Send(new GetTeacherPortalPayrollQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalTeacher);

        teacher.MapGet("/timetable", async (ISender sender) =>

        {

            var result = await sender.Send(new GetTeacherPortalTimetableQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalTeacher);



        var parent = app.MapGroup("/parents/me").WithTags("Parent Portal");

        parent.MapGet("/", async (ISender sender) =>

        {

            var result = await sender.Send(new GetParentPortalProfileQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalParent);

        parent.MapGet("/children", async (ISender sender) =>

        {

            var result = await sender.Send(new GetParentPortalChildrenQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalParent);

        parent.MapGet("/children/{childId}/fees", async (string childId, ISender sender) =>

        {

            var result = await sender.Send(new GetParentPortalChildFeesQuery(childId));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalParent);

        parent.MapGet("/children/{childId}/attendance", async (string childId, ISender sender) =>

        {

            var result = await sender.Send(new GetParentPortalChildAttendanceQuery(childId));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalParent);

        parent.MapGet("/children/{childId}/transport", async (string childId, ISender sender) =>

        {

            var result = await sender.Send(new GetParentPortalChildTransportQuery(childId));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PortalParent);

    }

}


