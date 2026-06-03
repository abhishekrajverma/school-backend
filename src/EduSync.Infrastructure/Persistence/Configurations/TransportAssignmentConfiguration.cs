using EduSync.Modules.Transport.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class TransportAssignmentConfiguration : IEntityTypeConfiguration<TransportAssignment>
{
    public void Configure(EntityTypeBuilder<TransportAssignment> builder)
    {
        builder.ToTable("Assignments", "transport");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.StudentExternalId });
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
