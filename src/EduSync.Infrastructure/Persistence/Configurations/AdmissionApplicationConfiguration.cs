using EduSync.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class AdmissionApplicationConfiguration : IEntityTypeConfiguration<AdmissionApplication>
{
    public void Configure(EntityTypeBuilder<AdmissionApplication> builder)
    {
        builder.ToTable("Applications", "admissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.ApplicationNo }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.RegistrationId });
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.AcademicYearId });
        builder.Property(x => x.Source).HasMaxLength(16).IsRequired();
        builder.HasOne(x => x.Registration).WithMany().HasForeignKey(x => x.RegistrationId);
        builder.Property(x => x.FormDataJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.DocumentsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
