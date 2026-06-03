using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Audit.Domain;
using EduSync.Modules.Compliance.Application;
using EduSync.Modules.Compliance.Domain;
using EduSync.Modules.Events.Domain;
using EduSync.Modules.Webhooks.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Application.Compliance;

public sealed class RetentionOptions
{
    public bool Enabled { get; set; } = true;
    public int RunIntervalHours { get; set; } = 24;
    public int DefaultAuditLogDays { get; set; } = 365;
    public int DefaultOutboxDays { get; set; } = 90;
    public int DefaultWebhookDeliveryDays { get; set; } = 30;
}

public sealed class ListRetentionPoliciesQueryHandler(EduSyncDbContext db, ITenantContext tenant, IOptions<RetentionOptions> defaults)
    : IRequestHandler<ListRetentionPoliciesQuery, Result<IReadOnlyList<RetentionPolicyDto>>>
{
    public async Task<Result<IReadOnlyList<RetentionPolicyDto>>> Handle(ListRetentionPoliciesQuery request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<IReadOnlyList<RetentionPolicyDto>>.Failure(Error.Forbidden("Tenant required."));
        }

        await EnsureDefaultsAsync(tenant.TenantId.Value, ct);
        var policies = await db.RetentionPolicies.AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.EntityType)
            .ToListAsync(ct);
        return Result<IReadOnlyList<RetentionPolicyDto>>.Success(
            policies.Select(p => new RetentionPolicyDto(p.ExternalId, p.EntityType, p.RetentionDays, p.IsEnabled)).ToList());
    }

    private async Task EnsureDefaultsAsync(Guid tenantId, CancellationToken ct)
    {
        var opts = defaults.Value;
        var types = new[]
        {
            (RetentionEntityTypes.AuditLogs, opts.DefaultAuditLogDays),
            (RetentionEntityTypes.OutboxProcessed, opts.DefaultOutboxDays),
            (RetentionEntityTypes.WebhookDeliveries, opts.DefaultWebhookDeliveryDays),
        };
        foreach (var (entityType, days) in types)
        {
            var exists = await db.RetentionPolicies.AnyAsync(
                p => p.TenantId == tenantId && p.EntityType == entityType && !p.IsDeleted, ct);
            if (exists) continue;
            db.RetentionPolicies.Add(new RetentionPolicy
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                EntityType = entityType,
                RetentionDays = days,
                IsEnabled = true,
                UpdatedAtPolicy = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync(ct);
    }
}

public sealed class UpsertRetentionPolicyCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<UpsertRetentionPolicyCommand, Result<RetentionPolicyDto>>
{
    public async Task<Result<RetentionPolicyDto>> Handle(UpsertRetentionPolicyCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<RetentionPolicyDto>.Failure(Error.Forbidden("Tenant required."));
        }

        var body = request.Request;
        if (body.RetentionDays < 7)
        {
            return Result<RetentionPolicyDto>.Failure(Error.Validation("Retention must be at least 7 days."));
        }

        var policy = await db.RetentionPolicies.FirstOrDefaultAsync(
            p => p.TenantId == tenant.TenantId && p.EntityType == body.EntityType && !p.IsDeleted, ct);
        if (policy is null)
        {
            policy = new RetentionPolicy
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                EntityType = body.EntityType,
                CreatedAt = DateTime.UtcNow,
            };
            db.RetentionPolicies.Add(policy);
        }

        policy.RetentionDays = body.RetentionDays;
        policy.IsEnabled = body.IsEnabled;
        policy.UpdatedAtPolicy = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result<RetentionPolicyDto>.Success(
            new RetentionPolicyDto(policy.ExternalId, policy.EntityType, policy.RetentionDays, policy.IsEnabled));
    }
}

public sealed class RunRetentionCleanupCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<RunRetentionCleanupCommand, Result<RetentionCleanupResult>>
{
    public async Task<Result<RetentionCleanupResult>> Handle(RunRetentionCleanupCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<RetentionCleanupResult>.Failure(Error.Forbidden("Tenant required."));
        }

        var result = await RetentionCleanupExecutor.RunForTenantAsync(db, tenant.TenantId.Value, ct);
        return Result<RetentionCleanupResult>.Success(result);
    }
}

internal static class RetentionCleanupExecutor
{
    public static async Task<RetentionCleanupResult> RunForTenantAsync(
        EduSyncDbContext db,
        Guid tenantId,
        CancellationToken ct)
    {
        var policies = await db.RetentionPolicies.AsNoTracking()
            .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.IsEnabled)
            .ToListAsync(ct);

        var auditDays = policies.FirstOrDefault(p => p.EntityType == RetentionEntityTypes.AuditLogs)?.RetentionDays ?? 365;
        var outboxDays = policies.FirstOrDefault(p => p.EntityType == RetentionEntityTypes.OutboxProcessed)?.RetentionDays ?? 90;
        var webhookDays = policies.FirstOrDefault(p => p.EntityType == RetentionEntityTypes.WebhookDeliveries)?.RetentionDays ?? 30;

        var auditCutoff = DateTime.UtcNow.AddDays(-auditDays);
        var outboxCutoff = DateTime.UtcNow.AddDays(-outboxDays);
        var webhookCutoff = DateTime.UtcNow.AddDays(-webhookDays);

        var auditDeleted = await db.AuditLogEntries
            .Where(a => a.TenantId == tenantId && a.OccurredAt < auditCutoff)
            .ExecuteDeleteAsync(ct);

        var outboxDeleted = await db.OutboxMessages.IgnoreQueryFilters()
            .Where(m => m.TenantId == tenantId && m.Status == OutboxStatuses.Processed && m.ProcessedAt < outboxCutoff)
            .ExecuteDeleteAsync(ct);

        var webhookDeleted = await db.WebhookDeliveries.IgnoreQueryFilters()
            .Where(d => d.TenantId == tenantId && d.CreatedAt < webhookCutoff)
            .ExecuteDeleteAsync(ct);

        return new RetentionCleanupResult(auditDeleted, outboxDeleted, webhookDeleted);
    }
}
