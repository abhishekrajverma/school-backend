using EduSync.Api.Extensions;
using EduSync.Modules.Identity.Authorization;
using EduSync.Modules.Tenancy.Application.Dtos;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class FinancialYearEndpoints
{
    public static RouteGroupBuilder MapFinancialYearEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/financial-year-settings").WithTags("FinancialYear");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetFinancialYearSettingsQuery());
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.FinancialYearRead);

        group.MapPut("/current", async (SetCurrentFinancialYearRequest body, ISender sender) =>
        {
            var result = await sender.Send(new SetCurrentFinancialYearCommand(body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.FinancialYearWrite);

        group.MapPost("/years", async (CreateAcademicYearRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateAcademicYearCommand(body));
            return result.ToHttpResult(dto => Results.Created($"/api/financial-year-settings/years/{dto!.Id}", dto));
        }).RequirePermission(Permissions.FinancialYearWrite);

        group.MapPost("/years/{id}/close", async (string id, ISender sender) =>
        {
            var result = await sender.Send(new CloseAcademicYearCommand(id));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.FinancialYearWrite);

        return group;
    }
}
