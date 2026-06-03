using EduSync.Api.Extensions;

using EduSync.Modules.Identity.Authorization;

using EduSync.Modules.Library.Application;

using EduSync.SharedKernel.Pagination;

using MediatR;



namespace EduSync.Api.Endpoints;



public static class LibraryEndpoints

{

    public static void MapLibraryEndpoints(this IEndpointRouteBuilder app)

    {

        var books = app.MapGroup("/library/books").WithTags("Library");



        books.MapGet("/", async (int? page, int? pageSize, string? search, string? category, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, search, null, null);

            var result = await sender.Send(new ListBooksQuery(pagination, category));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.LibraryRead);



        books.MapGet("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new GetBookByIdQuery(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.LibraryRead);



        books.MapPost("/", async (CreateBookRequest body, ISender sender) =>

        {

            var result = await sender.Send(new CreateBookCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/library/books/{dto!.Id}", dto));

        }).RequirePermission(Permissions.LibraryWrite);



        books.MapPut("/{id}", async (string id, UpdateBookRequest body, ISender sender) =>

        {

            var result = await sender.Send(new UpdateBookCommand(id, body));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.LibraryWrite);



        books.MapDelete("/{id}", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new DeleteBookCommand(id));

            return result.ToHttpResult();

        }).RequirePermission(Permissions.LibraryWrite);



        var issues = app.MapGroup("/library/issues").WithTags("Library");



        issues.MapGet("/", async (int? page, int? pageSize, string? status, string? memberId, ISender sender) =>

        {

            var pagination = PaginationQuery.FromHttp(page, pageSize, null, null, null);

            var result = await sender.Send(new ListBookIssuesQuery(pagination, status, memberId));

            if (!result.IsSuccess) return result.ToHttpResult();

            var p = result.Value!;

            return Results.Ok(new { items = p.Items, page = p.Page, pageSize = p.PageSize, totalCount = p.TotalCount, totalPages = p.TotalPages });

        }).RequirePermission(Permissions.LibraryRead);



        issues.MapPost("/", async (IssueBookRequest body, ISender sender) =>

        {

            var result = await sender.Send(new IssueBookCommand(body));

            return result.ToHttpResult(dto => Results.Created($"/api/library/issues/{dto!.Id}", dto));

        }).RequirePermission(Permissions.LibraryWrite);



        issues.MapPost("/{id}/return", async (string id, ISender sender) =>

        {

            var result = await sender.Send(new ReturnBookCommand(id));

            return result.ToHttpResult(dto => Results.Ok(dto));

        }).RequirePermission(Permissions.LibraryWrite);

    }

}


