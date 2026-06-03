using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Events.Application;

public sealed record OutboxMessageDto(
    string Id,
    string EventType,
    string Status,
    Guid? TenantId,
    string? Region,
    string? CorrelationId,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    int Attempts,
    string? Error);

public sealed record ListOutboxMessagesQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<OutboxMessageDto>>>;
