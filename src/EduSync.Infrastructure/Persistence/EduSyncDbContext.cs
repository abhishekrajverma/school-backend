using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Audit.Domain;
using EduSync.Modules.Compliance.Domain;
using EduSync.Modules.Events.Domain;
using EduSync.Modules.Webhooks.Domain;
using EduSync.Modules.Admissions.Domain;
using EduSync.Modules.Academics.Domain;
using EduSync.Modules.Attendance.Domain;
using EduSync.Modules.Exams.Domain;
using EduSync.Modules.Fees.Domain;
using EduSync.Modules.Notifications.Domain;
using EduSync.Modules.Payroll.Domain;
using EduSync.Modules.Leave.Domain;
using EduSync.Modules.Library.Domain;
using EduSync.Modules.Transport.Domain;
using EduSync.Modules.Hostel.Domain;
using EduSync.Modules.Inventory.Domain;
using EduSync.Modules.Uploads.Domain;
using EduSync.Modules.Jobs.Domain;
using EduSync.Modules.Timetable.Domain;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Parents.Domain;
using EduSync.Modules.Staff.Domain;
using EduSync.Modules.Students.Domain;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Abstractions;
using EduSync.SharedKernel.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Persistence;

public sealed class EduSyncDbContext(
    DbContextOptions<EduSyncDbContext> options,
    ITenantContext tenantContext,
    IIntegrationEventCollector eventCollector) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<User> Users => Set<User>();
    public DbSet<TenantMembership> TenantMemberships => Set<TenantMembership>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<AdmissionApplication> AdmissionApplications => Set<AdmissionApplication>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<FeeInvoice> FeeInvoices => Set<FeeInvoice>();
    public DbSet<FeePayment> FeePayments => Set<FeePayment>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<TimetableEntry> TimetableEntries => Set<TimetableEntry>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookIssue> BookIssues => Set<BookIssue>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<TransportRoute> TransportRoutes => Set<TransportRoute>();
    public DbSet<HostelRoom> HostelRooms => Set<HostelRoom>();
    public DbSet<HostelAllocation> HostelAllocations => Set<HostelAllocation>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();
    public DbSet<TransportAssignment> TransportAssignments => Set<TransportAssignment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EduSyncDbContext).Assembly);
        ApplyTenantQueryFilters(modelBuilder);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(EduSyncDbContext)
                    .GetMethod(nameof(SetTenantAndSoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(null, [modelBuilder, tenantContext]);
                continue;
            }

            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var tenantOnly = typeof(EduSyncDbContext)
                .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);
            tenantOnly.Invoke(null, [modelBuilder, tenantContext]);
        }
    }

    private static void SetTenantFilter<TEntity>(ModelBuilder modelBuilder, ITenantContext tenantContext)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            tenantContext.TenantId != null && e.TenantId == tenantContext.TenantId);
    }

    private static void SetTenantAndSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder, ITenantContext tenantContext)
        where TEntity : TenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            tenantContext.TenantId != null && e.TenantId == tenantContext.TenantId && !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is SharedKernel.Entities.AuditableEntity auditable && entry.State is EntityState.Added)
            {
                auditable.CreatedAt = now;
            }

            if (entry.Entity is SharedKernel.Entities.AuditableEntity updated && entry.State is EntityState.Modified)
            {
                updated.UpdatedAt = now;
            }
        }

        var events = eventCollector.Drain();
        foreach (var evt in events)
        {
            OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                TenantId = evt.TenantId,
                EventType = evt.EventType,
                Payload = evt.Payload,
                Region = evt.Region,
                CorrelationId = evt.CorrelationId,
                Status = OutboxStatuses.Pending,
                CreatedAt = DateTime.UtcNow,
            });
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
