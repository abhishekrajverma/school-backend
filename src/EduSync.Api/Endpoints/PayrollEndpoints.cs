using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Payroll.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class PayrollEndpoints

{

    public static RouteGroupBuilder MapPayrollEndpoints(this IEndpointRouteBuilder app)

    {

        var group = app.MapGroup("/payroll").WithTags("Payroll");



        group.MapGet("/", async (int? page, int? pageSize, string? month, int? year, string? status, string? employeeId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListPayrollQuery(pagination, month, year, status, employeeId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.PayrollRead);



        group.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetPayrollByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PayrollRead);



        group.MapPost("/", async (CreatePayrollRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreatePayrollCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/payroll/{dto!.Id}", dto));

        }).RequirePermission(Permissions.PayrollWrite);



        group.MapPost("/{id}/process", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new ProcessPayrollCommand(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.PayrollProcess);



        return group;

    }

}


