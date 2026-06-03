using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Imports.Application;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class ImportEndpoints

{

    public static RouteGroupBuilder MapImportEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/imports").WithTags("Imports").RequirePermission(Permissions.ImportsRun).DisableAntiforgery();



        group.MapGet("/students/template", () =>

        {

            const string csv = "FirstName,LastName,Email,Class,Section,RollNo,AdmissionNo,Phone,ParentEmail\n";

            return Results.File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "students-import-template.csv");

        });



        group.MapGet("/teachers/template", () =>

        {

            const string csv = "FirstName,LastName,Email,EmployeeId,Department,Subject,Qualification,Experience,Salary,Phone\n";

            return Results.File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "teachers-import-template.csv");

        });



        group.MapPost("/students", async (IFormFile file, ISender sender) =>

        {

            if (file is null || file.Length == 0)

                return Results.Json(new { code = "VALIDATION_ERROR", message = "CSV file is required." }, statusCode: 400);

            await using var stream = file.OpenReadStream();

            var result = await sender.Send(new ImportStudentsCsvCommand(stream));

            return result.ToHttpResult(dto => Results.Ok(dto));

        });



        group.MapPost("/teachers", async (IFormFile file, ISender sender) =>

        {

            if (file is null || file.Length == 0)

                return Results.Json(new { code = "VALIDATION_ERROR", message = "CSV file is required." }, statusCode: 400);

            await using var stream = file.OpenReadStream();

            var result = await sender.Send(new ImportTeachersCsvCommand(stream));

            return result.ToHttpResult(dto => Results.Ok(dto));

        });



        group.MapPost("/students/queue", async (QueueImportBody body, ISender sender) =>

        {

            var result = await sender.Send(new QueueImportStudentsCommand(body.FileId));

            return result.ToHttpResult(jobId => Results.Accepted($"/hangfire/jobs/details/{jobId}", new { hangfireJobId = jobId }));

        });



        group.MapPost("/teachers/queue", async (QueueImportBody body, ISender sender) =>

        {

            var result = await sender.Send(new QueueImportTeachersCommand(body.FileId));

            return result.ToHttpResult(jobId => Results.Accepted($"/hangfire/jobs/details/{jobId}", new { hangfireJobId = jobId }));

        });



        return group;

    }



    private sealed record QueueImportBody(string FileId);

}


