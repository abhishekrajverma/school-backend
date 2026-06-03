using EduSync.Modules.Uploads.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("Files", "uploads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
