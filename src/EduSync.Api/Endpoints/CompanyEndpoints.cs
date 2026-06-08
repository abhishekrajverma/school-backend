using EduSync.Api.Extensions;
using EduSync.Modules.Company.Application;
using EduSync.Modules.Identity.Authorization;
using MediatR;

namespace EduSync.Api.Endpoints;

public static class CompanyEndpoints
{
    public static RouteGroupBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/company").WithTags("Company");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCompanyOverviewQuery());
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.CompanyRead);

        group.MapPost("/", async (CompanyActionRequest body, ISender sender) =>
        {
            var result = await sender.Send(new ExecuteCompanyActionCommand(body));
            return result.ToHttpResult(dto => Results.Ok(dto));
        }).RequirePermission(Permissions.TenantsManage);

        return group;
    }
}
