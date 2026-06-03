using EduSync.Infrastructure.Application.Imports;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Imports.Application;
using EduSync.Modules.Jobs.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduSync.Infrastructure.Hangfire;

public sealed class HangfireBulkImportJob(IServiceProvider serviceProvider)
{
    public async Task ImportStudentsAsync(Guid tenantId, string tenantSlug, string? tenantExternalId, string fileExternalId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantContext = (TenantContext)scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.Set(tenantId, tenantSlug, tenantExternalId);

        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        var file = await db.StoredFiles.FirstOrDefaultAsync(f => f.ExternalId == fileExternalId && !f.IsDeleted)
            ?? throw new InvalidOperationException($"Upload {fileExternalId} not found.");

        var storage = scope.ServiceProvider.GetRequiredService<Infrastructure.Storage.IFileStorageService>();
        var (stream, _, dispose) = await storage.OpenReadAsync(file.StoragePath);
        await using (stream)
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(new ImportStudentsCsvCommand(stream));
            await RecordImportJobAsync(db, tenantId, "bulk_import_students", result);
        }

        if (dispose) await stream.DisposeAsync();
    }

    public async Task ImportTeachersAsync(Guid tenantId, string tenantSlug, string? tenantExternalId, string fileExternalId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantContext = (TenantContext)scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.Set(tenantId, tenantSlug, tenantExternalId);

        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();
        var file = await db.StoredFiles.FirstOrDefaultAsync(f => f.ExternalId == fileExternalId && !f.IsDeleted)
            ?? throw new InvalidOperationException($"Upload {fileExternalId} not found.");

        var storage = scope.ServiceProvider.GetRequiredService<Infrastructure.Storage.IFileStorageService>();
        var (stream, _, dispose) = await storage.OpenReadAsync(file.StoragePath);
        await using (stream)
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            var result = await sender.Send(new ImportTeachersCsvCommand(stream));
            await RecordImportJobAsync(db, tenantId, "bulk_import_teachers", result);
        }

        if (dispose) await stream.DisposeAsync();
    }

    private static async Task RecordImportJobAsync(
        EduSyncDbContext db, Guid tenantId, string jobType, EduSync.SharedKernel.Results.Result<ImportResultDto> result)
    {
        var execution = new JobExecution
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            JobType = jobType,
            Status = result.IsSuccess ? "completed" : "failed",
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            ItemsProcessed = result.IsSuccess ? result.Value!.Imported : 0,
            Message = result.IsSuccess
                ? $"Imported {result.Value!.Imported}, skipped {result.Value.Skipped}, failed {result.Value.Failed}."
                : result.Error?.Message,
        };
        db.JobExecutions.Add(execution);
        await db.SaveChangesAsync();
    }
}
