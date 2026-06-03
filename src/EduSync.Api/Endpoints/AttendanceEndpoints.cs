using EduSync.Api.Extensions;

using EduSync.Modules.Attendance.Application;

using EduSync.Modules.Identity.Authorization;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class AttendanceEndpoints

{

    public static RouteGroupBuilder MapAttendanceEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/attendance").WithTags("Attendance");



        group.MapGet("/", async (int? page, int? pageSize, string? search, string? date, string? entityType, string? className, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);

            var result = await sender.Send(new ListAttendanceQuery(pagination, date, entityType, className));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.AttendanceRead);



        group.MapGet("/students/{studentId}", async (string studentId, string? from, string? to, ISender sender) =>

        {

            DateOnly? fromDate = DateOnly.TryParse(from, out var f) ? f : null;

            DateOnly? toDate = DateOnly.TryParse(to, out var t) ? t : null;

            var result = await sender.Send(new GetStudentAttendanceQuery(studentId, fromDate, toDate));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AttendanceRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetAttendanceByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AttendanceRead);



        group.MapPost("/", async (MarkAttendanceRequest body, ISender sender) =>

        {

            var result = await sender.Send(new MarkAttendanceCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/attendance/{dto!.Id}", dto));

        }).RequirePermission(Permissions.AttendanceWrite);



        group.MapPost("/bulk", async (BulkMarkAttendanceRequest body, ISender sender) =>

        {

            var result = await sender.Send(new BulkMarkAttendanceCommand(body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.AttendanceWrite);



        return group;

    }

}


