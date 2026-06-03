using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Library.Application;
using EduSync.Modules.Library.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Library;

internal static class LibraryMapping
{
    public static BookDto ToDto(Book b) => new(
        b.ExternalId, b.Title, b.Author, b.Isbn, b.Category, b.Publisher, b.PublishYear,
        b.Quantity, b.Available, b.Issued, b.Location, b.Description);

    public static BookIssueDto ToIssueDto(BookIssue i) => new(
        i.ExternalId, i.BookExternalId, i.BookTitle, i.MemberExternalId, i.MemberName,
        i.MemberType, i.ClassName, i.IssueDate.ToString("yyyy-MM-dd"), i.DueDate.ToString("yyyy-MM-dd"),
        i.ReturnDate?.ToString("yyyy-MM-dd"), i.Status, i.Fine);
}

public sealed class ListBooksQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListBooksQuery, Result<PaginatedList<BookDto>>>
{
    public async Task<Result<PaginatedList<BookDto>>> Handle(ListBooksQuery request, CancellationToken ct)
    {
        var query = db.Books.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category == request.Category);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(x => x.Title.ToLower().Contains(term) || x.Author.ToLower().Contains(term));
        }
        query = query.OrderBy(x => x.Title);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(LibraryMapping.ToDto).ToList();
        return Result<PaginatedList<BookDto>>.Success(
            PaginatedList<BookDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetBookByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetBookByIdQuery, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(GetBookByIdQuery request, CancellationToken ct)
    {
        var b = await db.Books.AsNoTracking().FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return b is null ? Result<BookDto>.Failure(Error.NotFound("Book not found."))
            : Result<BookDto>.Success(LibraryMapping.ToDto(b));
    }
}

public sealed class CreateBookCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(CreateBookCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<BookDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        var book = new Book
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            Title = b.Title, Author = b.Author, Isbn = b.Isbn, Category = b.Category,
            Publisher = b.Publisher, PublishYear = b.PublishYear,
            Quantity = b.Quantity, Available = b.Available, Issued = b.Quantity - b.Available,
            Location = b.Location, Description = b.Description,
        };
        db.Books.Add(book);
        await db.SaveChangesAsync(ct);
        return Result<BookDto>.Success(LibraryMapping.ToDto(book));
    }
}

public sealed class UpdateBookCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateBookCommand, Result<BookDto>>
{
    public async Task<Result<BookDto>> Handle(UpdateBookCommand request, CancellationToken ct)
    {
        var book = await db.Books.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (book is null) return Result<BookDto>.Failure(Error.NotFound("Book not found."));
        var b = request.Request;
        book.Title = b.Title; book.Author = b.Author; book.Isbn = b.Isbn; book.Category = b.Category;
        book.Publisher = b.Publisher; book.PublishYear = b.PublishYear; book.Quantity = b.Quantity;
        book.Available = Math.Min(b.Quantity, book.Available);
        book.Issued = book.Quantity - book.Available;
        book.Location = b.Location; book.Description = b.Description;
        await db.SaveChangesAsync(ct);
        return Result<BookDto>.Success(LibraryMapping.ToDto(book));
    }
}

public sealed class DeleteBookCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteBookCommand, Result>
{
    public async Task<Result> Handle(DeleteBookCommand request, CancellationToken ct)
    {
        var book = await db.Books.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (book is null) return Result.Failure(Error.NotFound("Book not found."));
        book.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class ListBookIssuesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListBookIssuesQuery, Result<PaginatedList<BookIssueDto>>>
{
    public async Task<Result<PaginatedList<BookIssueDto>>> Handle(ListBookIssuesQuery request, CancellationToken ct)
    {
        var query = db.BookIssues.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.MemberId)) query = query.Where(x => x.MemberExternalId == request.MemberId);
        query = query.OrderByDescending(x => x.IssueDate);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(LibraryMapping.ToIssueDto).ToList();
        return Result<PaginatedList<BookIssueDto>>.Success(
            PaginatedList<BookIssueDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class IssueBookCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<IssueBookCommand, Result<BookIssueDto>>
{
    public async Task<Result<BookIssueDto>> Handle(IssueBookCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<BookIssueDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.IssueDate, out var issueDate) || !DateOnly.TryParse(b.DueDate, out var dueDate))
            return Result<BookIssueDto>.Failure(Error.Validation("Invalid dates."));
        var book = await db.Books.FirstOrDefaultAsync(x => x.ExternalId == b.BookId && !x.IsDeleted, ct);
        if (book is null) return Result<BookIssueDto>.Failure(Error.NotFound("Book not found."));
        if (book.Available <= 0) return Result<BookIssueDto>.Failure(Error.Validation("No copies available."));
        book.Available--;
        book.Issued++;
        var issue = new BookIssue
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            BookId = book.Id, BookExternalId = book.ExternalId, BookTitle = book.Title,
            MemberExternalId = b.MemberId, MemberName = b.MemberName, MemberType = b.MemberType,
            ClassName = b.Class, IssueDate = issueDate, DueDate = dueDate, Status = "issued",
        };
        db.BookIssues.Add(issue);
        await db.SaveChangesAsync(ct);
        return Result<BookIssueDto>.Success(LibraryMapping.ToIssueDto(issue));
    }
}

public sealed class ReturnBookCommandHandler(EduSyncDbContext db)
    : IRequestHandler<ReturnBookCommand, Result<BookIssueDto>>
{
    public async Task<Result<BookIssueDto>> Handle(ReturnBookCommand request, CancellationToken ct)
    {
        var issue = await db.BookIssues.Include(i => i.Book)
            .FirstOrDefaultAsync(x => x.ExternalId == request.IssueExternalId && !x.IsDeleted, ct);
        if (issue is null) return Result<BookIssueDto>.Failure(Error.NotFound("Issue not found."));
        if (issue.Status == "returned") return Result<BookIssueDto>.Success(LibraryMapping.ToIssueDto(issue));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        issue.ReturnDate = today;
        issue.Status = issue.DueDate < today ? "returned" : "returned";
        if (issue.DueDate < today && issue.Fine == 0) issue.Fine = 50;
        issue.Book.Available++;
        issue.Book.Issued = Math.Max(0, issue.Book.Issued - 1);
        await db.SaveChangesAsync(ct);
        return Result<BookIssueDto>.Success(LibraryMapping.ToIssueDto(issue));
    }
}
