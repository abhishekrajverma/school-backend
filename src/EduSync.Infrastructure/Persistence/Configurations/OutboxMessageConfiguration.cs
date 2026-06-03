using EduSync.Modules.Events.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("Messages", "events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
        builder.Property(x => x.EventType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.Region).HasMaxLength(32);
        builder.Property(x => x.CorrelationId).HasMaxLength(64);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
    }
}
