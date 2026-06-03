using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Library.Application;

public sealed record BookDto(
    string Id, string Title, string Author, string Isbn, string Category,
    string? Publisher, int? PublishYear, int Quantity, int Available, int Issued,
    string? Location, string? Description);

public sealed record CreateBookRequest(
    string Title, string Author, string Isbn, string Category,
    string? Publisher, int? PublishYear, int Quantity, int Available,
    string? Location, string? Description);

public sealed record UpdateBookRequest(
    string Title, string Author, string Isbn, string Category,
    string? Publisher, int? PublishYear, int Quantity,
    string? Location, string? Description);

public sealed record BookIssueDto(
    string Id, string BookId, string BookTitle, string MemberId, string MemberName,
    string MemberType, string? Class, string IssueDate, string DueDate, string? ReturnDate,
    string Status, decimal Fine);

public sealed record IssueBookRequest(
    string BookId, string MemberId, string MemberName, string MemberType,
    string? Class, string IssueDate, string DueDate, string? Remarks);

public sealed record ListBooksQuery(PaginationQuery Pagination, string? Category)
    : IRequest<Result<PaginatedList<BookDto>>>;

public sealed record GetBookByIdQuery(string ExternalId) : IRequest<Result<BookDto>>;
public sealed record CreateBookCommand(CreateBookRequest Request) : IRequest<Result<BookDto>>;
public sealed record UpdateBookCommand(string ExternalId, UpdateBookRequest Request) : IRequest<Result<BookDto>>;
public sealed record DeleteBookCommand(string ExternalId) : IRequest<Result>;

public sealed record ListBookIssuesQuery(PaginationQuery Pagination, string? Status, string? MemberId)
    : IRequest<Result<PaginatedList<BookIssueDto>>>;

public sealed record IssueBookCommand(IssueBookRequest Request) : IRequest<Result<BookIssueDto>>;
public sealed record ReturnBookCommand(string IssueExternalId) : IRequest<Result<BookIssueDto>>;
