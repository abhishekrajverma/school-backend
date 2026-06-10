using EduSync.Api.Extensions;

using EduSync.Modules.Exams.Application;

using EduSync.Modules.Identity.Authorization;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class ExamEndpoints

{

    public static RouteGroupBuilder MapExamEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/exams").WithTags("Exams");



        group.MapGet("/", async (int? page, int? pageSize, string? search, string? className, string? status, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);

            var result = await sender.Send(new ListExamsQuery(pagination, className, status));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.ExamsRead);

        group.MapGet("/results", async (string? examId, string? studentId, Guid? academicYearId, ISender sender) =>
        {
            var result = await sender.Send(new ListExamResultsQuery(examId, studentId, academicYearId));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.ExamsRead);

        group.MapPost("/results", async (RecordExamResultRequest body, ISender sender) =>
        {
            var result = await sender.Send(new RecordExamResultCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/exams/results/{dto!.Id}", dto));
        }).RequirePermission(Permissions.ExamsWrite);

        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetExamByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.ExamsRead);



        group.MapPost("/", async (CreateExamRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateExamCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/exams/{dto!.Id}", dto));

        }).RequirePermission(Permissions.ExamsWrite);



        group.MapPut("/{id}", async (string id, UpdateExamRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateExamCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.ExamsWrite);



        group.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteExamCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.ExamsWrite);

        return group;

    }

}


