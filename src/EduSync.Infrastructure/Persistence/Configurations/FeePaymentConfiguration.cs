using EduSync.Modules.Fees.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class FeePaymentConfiguration : IEntityTypeConfiguration<FeePayment>
{
    public void Configure(EntityTypeBuilder<FeePayment> builder)
    {
        builder.ToTable("Payments", "fees");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.StudentExternalId });
        builder.Property(x => x.StudentExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.HasOne(x => x.Invoice).WithMany().HasForeignKey(x => x.FeeInvoiceId);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
