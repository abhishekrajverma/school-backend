using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Jobs.Application;

public sealed record JobRunDto(
    string Id,
    string JobType,
    string Status,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string? Message,
    int ItemsProcessed);

public sealed record ListJobRunsQuery(PaginationQuery Pagination, string? JobType)
    : IRequest<Result<PaginatedList<JobRunDto>>>;

public sealed record RunFeeReminderJobCommand : IRequest<Result<JobRunDto>>;
