using EduSync.Modules.Fees.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class FeeInvoiceConfiguration : IEntityTypeConfiguration<FeeInvoice>
{
    public void Configure(EntityTypeBuilder<FeeInvoice> builder)
    {
        builder.ToTable("Invoices", "fees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.InvoiceNo }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.StudentExternalId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.Property(x => x.StudentExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.StudentName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ClassName).HasMaxLength(32).IsRequired();
        builder.Property(x => x.FeeType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.TotalFee).HasPrecision(18, 2);
        builder.Property(x => x.Paid).HasPrecision(18, 2);
        builder.Property(x => x.Pending).HasPrecision(18, 2);
        builder.Property(x => x.Discount).HasPrecision(18, 2);
        builder.Property(x => x.Fine).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
