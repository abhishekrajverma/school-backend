using EduSync.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("Items", "inventory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Sku }).IsUnique();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
