using EduSync.Infrastructure.Jobs;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Jobs.Application;
using EduSync.Modules.Jobs.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Jobs;

internal static class JobMapping
{
    public static JobRunDto ToDto(JobExecution j) => new(
        j.ExternalId, j.JobType, j.Status, j.StartedAt, j.CompletedAt, j.Message, j.ItemsProcessed);
}

public sealed class ListJobRunsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListJobRunsQuery, Result<PaginatedList<JobRunDto>>>
{
    public async Task<Result<PaginatedList<JobRunDto>>> Handle(ListJobRunsQuery request, CancellationToken ct)
    {
        var query = db.JobExecutions.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.JobType)) query = query.Where(x => x.JobType == request.JobType);
        query = query.OrderByDescending(x => x.StartedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(JobMapping.ToDto).ToList();
        return Result<PaginatedList<JobRunDto>>.Success(
            PaginatedList<JobRunDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class RunFeeReminderJobCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IFeeReminderJob feeReminderJob)
    : IRequestHandler<RunFeeReminderJobCommand, Result<JobRunDto>>
{
    public async Task<Result<JobRunDto>> Handle(RunFeeReminderJobCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<JobRunDto>.Failure(Error.Forbidden("Tenant required."));

        var execution = new JobExecution
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            JobType = FeeReminderJob.JobType,
            Status = "running",
            StartedAt = DateTime.UtcNow,
        };
        db.JobExecutions.Add(execution);
        await db.SaveChangesAsync(ct);

        try
        {
            var count = await feeReminderJob.RunAsync(ct);
            execution.Status = "completed";
            execution.CompletedAt = DateTime.UtcNow;
            execution.ItemsProcessed = count;
            execution.Message = $"Created {count} fee reminder notification(s).";
        }
        catch (Exception ex)
        {
            execution.Status = "failed";
            execution.CompletedAt = DateTime.UtcNow;
            execution.Message = ex.Message;
            await db.SaveChangesAsync(ct);
            return Result<JobRunDto>.Failure(Error.Validation($"Job failed: {ex.Message}"));
        }

        await db.SaveChangesAsync(ct);
        return Result<JobRunDto>.Success(JobMapping.ToDto(execution));
    }
}
