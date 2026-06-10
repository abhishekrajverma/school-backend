using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Events.Domain;
using EduSync.Modules.Fees.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EduSync.Infrastructure.Events;

/// <summary>
/// Initializes a pending fee invoice when a student is admitted.
/// </summary>
public sealed class AdmissionApprovedIntegrationHandler(
    IServiceScopeFactory scopeFactory,
    ILogger<AdmissionApprovedIntegrationHandler> logger) : IIntegrationEventHandler
{
    public bool CanHandle(string eventType) =>
        eventType == IntegrationEventTypes.AdmissionApproved;

    public async Task HandleAsync(
        string eventType,
        string payload,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EduSyncDbContext>();

        using var doc = System.Text.Json.JsonDocument.Parse(payload);
        var root = doc.RootElement;
        if (!root.TryGetProperty("studentExternalId", out var studentIdProp))
        {
            return;
        }

        var studentExternalId = studentIdProp.GetString();
        if (string.IsNullOrWhiteSpace(studentExternalId) || !tenantId.HasValue)
        {
            return;
        }

        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == studentExternalId && !s.IsDeleted, cancellationToken);
        if (student is null)
        {
            return;
        }

        var enrollment = await db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == student.Id && !e.IsDeleted)
            .OrderByDescending(e => e.EnrolledAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (enrollment is null)
        {
            return;
        }

        var exists = await db.FeeInvoices.AnyAsync(
            f => f.StudentExternalId == student.ExternalId && !f.IsDeleted,
            cancellationToken);
        if (exists)
        {
            return;
        }

        var year = await db.AcademicYears.AsNoTracking()
            .FirstOrDefaultAsync(y => y.Id == enrollment.AcademicYearId, cancellationToken);

        db.FeeInvoices.Add(new FeeInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId.Value,
            BranchId = enrollment.BranchId,
            AcademicYearId = enrollment.AcademicYearId,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FinancialYear = year?.Name ?? string.Empty,
            InvoiceNo = $"INV{DateTime.UtcNow:yyyy}{Random.Shared.Next(1000, 9999)}",
            StudentExternalId = student.ExternalId,
            StudentName = student.FullName,
            ClassName = enrollment.ClassName,
            TotalFee = 0,
            Pending = 0,
            DueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)),
            Status = "pending",
        });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Initialized fee account for student {StudentId} after admission", student.ExternalId);
    }
}
