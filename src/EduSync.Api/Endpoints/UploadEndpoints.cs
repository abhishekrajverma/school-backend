using System.Security.Claims;

using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Uploads.Application;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class UploadEndpoints

{

    public static RouteGroupBuilder MapUploadEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/uploads").WithTags("Uploads").DisableAntiforgery();



        group.MapPost("/", async (IFormFile file, string? category, ClaimsPrincipal user, ISender sender) =>

        {

            if (file is null || file.Length == 0)

            {

                return Results.Json(

                    new { code = "VALIDATION_ERROR", message = "File is required." },

                    statusCode: StatusCodes.Status400BadRequest);

            }



            Guid? userId = null;

            var sub = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

            if (sub is not null && Guid.TryParse(sub, out var id)) userId = id;



            await using var stream = file.OpenReadStream();

            var result = await sender.Send(new UploadFileCommand(

                stream, file.FileName, file.ContentType ?? "application/octet-stream",

                file.Length, category, userId));

            return result.ToHttpResult(dto => Results.Created($"/api/uploads/{dto!.Id}", dto));

        }).RequirePermission(Permissions.UploadsWrite);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetUploadByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.UploadsRead);



        group.MapGet("/{id}/download", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DownloadUploadQuery(id));

            if (!result.IsSuccess) return result.ToHttpResult();

            var file = result.Value!;

            return Results.File(file.Stream, file.ContentType, file.FileName);

        }).RequirePermission(Permissions.UploadsRead);



        return group;

    }

}


