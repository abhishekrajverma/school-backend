using EduSync.Modules.Audit.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("Logs", "audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.OccurredAt });
        builder.Property(x => x.Action).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Method).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(512).IsRequired();
    }
}
