using EduSync.Api.Extensions;

using EduSync.Modules.Compliance.Application;

using EduSync.Modules.Identity.Authorization;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class RetentionEndpoints

{

    public static RouteGroupBuilder MapRetentionEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/retention").WithTags("Data Retention").RequirePermission(Permissions.RetentionManage);



        group.MapGet("/policies", async (ISender sender) =>

        {

            var result = await sender.Send(new ListRetentionPoliciesQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        });



        group.MapPut("/policies", async (UpsertRetentionPolicyRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpsertRetentionPolicyCommand(body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        });



        group.MapPost("/run", async (ISender sender) =>

        {

            var result = await sender.Send(new RunRetentionCleanupCommand());

            return result.ToHttpResult(dto => Results.Ok(dto));

        });



        return group;

    }

}


