using EduSync.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications", "notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.TargetAudience });
        builder.Property(x => x.TargetAudience).HasMaxLength(32).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
