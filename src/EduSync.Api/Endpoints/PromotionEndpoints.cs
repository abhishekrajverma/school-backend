using EduSync.Api.Extensions;
using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Students.Application;
using EduSync.Modules.Students.Application.Dtos;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class PromotionEndpoints
{
    public static RouteGroupBuilder MapPromotionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/promotions").WithTags("Promotions");

        group.MapPost("/bulk", async (BulkPromoteRequest body, ISender sender) =>
        {
            var result = await sender.Send(new BulkPromoteStudentsCommand(body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.StudentsWrite);

        group.MapPost("/{batchId}/rollback", async (string batchId, ISender sender) =>
        {
            var result = await sender.Send(new RollbackPromotionBatchCommand(batchId));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.StudentsWrite);

        return group;
    }
}
