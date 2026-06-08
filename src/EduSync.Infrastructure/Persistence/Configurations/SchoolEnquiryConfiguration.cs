using EduSync.Modules.Company.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class SchoolEnquiryConfiguration : IEntityTypeConfiguration<SchoolEnquiry>
{
    public void Configure(EntityTypeBuilder<SchoolEnquiry> builder)
    {
        builder.ToTable("Enquiries", "company");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(32).IsRequired();
        builder.Property(x => x.SchoolName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(32);
        builder.Property(x => x.City).HasMaxLength(128);
        builder.Property(x => x.PlanKey).HasMaxLength(64);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.TenantExternalId).HasMaxLength(64);
        builder.HasIndex(x => x.ExternalId).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatedAt);
    }
}
