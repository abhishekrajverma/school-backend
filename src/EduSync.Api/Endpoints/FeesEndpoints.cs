using EduSync.Api.Extensions;

using EduSync.Modules.Fees.Application;

using EduSync.Modules.Identity.Authorization;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class FeesEndpoints

{

    public static RouteGroupBuilder MapFeesEndpoints(this IEndpointRouteBuilder app)

    {

        var fees = app.MapGroup("/fees").WithTags("Fees");



        fees.MapGet("/", async (int? page, int? pageSize, string? search, string? status, string? studentId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);

            var result = await sender.Send(new ListFeesQuery(pagination, status, studentId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.FeesRead);



        fees.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetFeeByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.FeesRead);



        fees.MapPost("/", async (CreateFeeRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateFeeCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/fees/{dto!.Id}", dto));

        }).RequirePermission(Permissions.FeesWrite);



        fees.MapPost("/{id}/payments", async (string id, RecordPaymentRequest body, ISender sender) =>

        {

            var result = await sender.Send(new RecordPaymentCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.FeesWrite);



        var payments = app.MapGroup("/payments").WithTags("Payments");

        payments.MapGet("/", async (int? page, int? pageSize, string? studentId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListPaymentsQuery(pagination, studentId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.PaymentsRead);



        return fees;

    }

}


