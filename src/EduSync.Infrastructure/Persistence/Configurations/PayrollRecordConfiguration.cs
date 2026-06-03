using EduSync.Modules.Payroll.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("Records", "payroll");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.Property(x => x.BasicSalary).HasPrecision(18, 2);
        builder.Property(x => x.Hra).HasPrecision(18, 2);
        builder.Property(x => x.Da).HasPrecision(18, 2);
        builder.Property(x => x.Ta).HasPrecision(18, 2);
        builder.Property(x => x.Medical).HasPrecision(18, 2);
        builder.Property(x => x.Special).HasPrecision(18, 2);
        builder.Property(x => x.PfDeduction).HasPrecision(18, 2);
        builder.Property(x => x.TaxDeduction).HasPrecision(18, 2);
        builder.Property(x => x.Insurance).HasPrecision(18, 2);
        builder.Property(x => x.LoanDeduction).HasPrecision(18, 2);
        builder.Property(x => x.OtherDeduction).HasPrecision(18, 2);
        builder.Property(x => x.Bonus).HasPrecision(18, 2);
        builder.Property(x => x.GrossSalary).HasPrecision(18, 2);
        builder.Property(x => x.TotalDeductions).HasPrecision(18, 2);
        builder.Property(x => x.NetSalary).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
