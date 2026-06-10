using EduSync.Modules.Admissions.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class RegistrationConfiguration : IEntityTypeConfiguration<Registration>
{
    public void Configure(EntityTypeBuilder<Registration> builder)
    {
        builder.ToTable("Registrations", "admissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RegistrationNo).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.RegistrationNo }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.ApplicantEmail, x.ApplicantPhone });
    }
}

internal sealed class RegistrationDocumentConfiguration : IEntityTypeConfiguration<RegistrationDocument>
{
    public void Configure(EntityTypeBuilder<RegistrationDocument> builder)
    {
        builder.ToTable("RegistrationDocuments", "admissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DocumentType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.HasOne(x => x.Registration).WithMany().HasForeignKey(x => x.RegistrationId);
    }
}

internal sealed class AdmissionApprovalConfiguration : IEntityTypeConfiguration<AdmissionApproval>
{
    public void Configure(EntityTypeBuilder<AdmissionApproval> builder)
    {
        builder.ToTable("AdmissionApprovals", "admissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Decision).HasMaxLength(32).IsRequired();
        builder.HasOne(x => x.Application).WithMany(a => a.Approvals).HasForeignKey(x => x.AdmissionApplicationId);
    }
}
