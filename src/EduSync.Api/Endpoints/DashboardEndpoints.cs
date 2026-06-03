using EduSync.Api.Extensions;

using EduSync.Modules.Dashboard.Application;

using EduSync.Modules.Identity.Authorization;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class DashboardEndpoints

{

    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)

    {

        var dashboard = app.MapGroup("/dashboard").WithTags("Dashboard");

        dashboard.MapGet("/", async (ISender sender) =>

        {

            var result = await sender.Send(new GetDashboardQuery());

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.DashboardRead);



        var reports = app.MapGroup("/reports").WithTags("Reports");

        reports.MapGet("/", async (string? type, string? from, string? to, ISender sender) =>

        {

            var result = await sender.Send(new GetReportQuery(type ?? "overview", from, to));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.ReportsRead);



        reports.MapGet("/export", async (string? format, string? type, string? from, string? to, ISender sender) =>

        {

            var result = await sender.Send(new ExportReportQuery(format ?? "csv", type ?? "fees", from, to));

            if (!result.IsSuccess) return result.ToHttpResult();

            var export = result.Value!;

            return Results.File(export.Content, export.ContentType, export.FileName);

        }).RequirePermission(Permissions.ReportsExport);

    }

}


