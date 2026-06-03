using EduSync.Infrastructure.Application.Notifications;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Realtime;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Jobs;

public sealed class FeeReminderJob(
    EduSyncDbContext db,
    ITenantContext tenant,
    INotificationRealtimePublisher realtime) : IFeeReminderJob
{
    public const string JobType = "fee_reminder";

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        if (!tenant.TenantId.HasValue)
        {
            throw new InvalidOperationException("Tenant context required for fee reminder job.");
        }

        var tenantId = tenant.TenantId.Value;
        var overdue = await db.FeeInvoices
            .Where(f => !f.IsDeleted && (f.Status == "overdue" || f.Status == "pending") && f.Pending > 0)
            .Take(50)
            .ToListAsync(cancellationToken);

        var count = 0;
        var created = new List<Notification>();
        foreach (var invoice in overdue)
        {
            var exists = await db.Notifications.AnyAsync(
                n => !n.IsDeleted && n.Title == "Fee Payment Reminder"
                     && n.Message.Contains(invoice.InvoiceNo),
                cancellationToken);
            if (exists) continue;

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ExternalId = Guid.NewGuid().ToString("N")[..8],
                Title = "Fee Payment Reminder",
                Message = $"Invoice {invoice.InvoiceNo} for {invoice.StudentName} has pending amount ₹{invoice.Pending:N0}. Due date {invoice.DueDate:yyyy-MM-dd}.",
                Type = "warning",
                TargetAudience = "parents",
                SentAt = DateTime.UtcNow,
                ReadCount = 0,
                TotalRecipients = 1,
            };
            db.Notifications.Add(notification);
            created.Add(notification);
            count++;
        }

        if (count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(tenant.TenantExternalId))
            {
                foreach (var n in created)
                {
                    await realtime.PublishCreatedAsync(
                        tenant.TenantExternalId,
                        n.TargetAudience,
                        NotificationMapping.ToDto(n),
                        cancellationToken);
                }
            }
        }

        return count;
    }
}
