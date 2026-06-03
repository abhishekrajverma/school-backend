using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Audit.Application;

public sealed record AuditLogDto(
    string Id,
    string Action,
    string Method,
    string Path,
    int StatusCode,
    string? UserEmail,
    string? EntityType,
    string? EntityId,
    DateTime OccurredAt);

public sealed record ListAuditLogsQuery(PaginationQuery Pagination, string? Action, string? Path)
    : IRequest<Result<PaginatedList<AuditLogDto>>>;
