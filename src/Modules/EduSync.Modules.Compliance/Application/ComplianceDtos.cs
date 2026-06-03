using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Compliance.Application;

public sealed record RetentionPolicyDto(string Id, string EntityType, int RetentionDays, bool IsEnabled);

public sealed record UpsertRetentionPolicyRequest(string EntityType, int RetentionDays, bool IsEnabled);

public sealed record ListRetentionPoliciesQuery : IRequest<Result<IReadOnlyList<RetentionPolicyDto>>>;

public sealed record UpsertRetentionPolicyCommand(UpsertRetentionPolicyRequest Request)
    : IRequest<Result<RetentionPolicyDto>>;

public sealed record RunRetentionCleanupCommand : IRequest<Result<RetentionCleanupResult>>;

public sealed record RetentionCleanupResult(int AuditLogsDeleted, int OutboxDeleted, int WebhookDeliveriesDeleted);
