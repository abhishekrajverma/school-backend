using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Jobs.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class JobEndpoints

{

    public static RouteGroupBuilder MapJobEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/jobs").WithTags("Jobs").RequirePermission(Permissions.JobsRun);



        group.MapGet("/runs", async (int? page, int? pageSize, string? jobType, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListJobRunsQuery(pagination, jobType));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        });



        group.MapPost("/fee-reminders", async (ISender sender) =>

        {

            var result = await sender.Send(new RunFeeReminderJobCommand());

            return result.ToHttpResult(dto => Results.Ok(dto));

        });



        return group;

    }

}


