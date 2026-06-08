using EduSync.Modules.Students.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students", "students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.AdmissionNo }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.Property(x => x.FinancialYear).HasMaxLength(16).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.FinancialYear });
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.RollNo).HasMaxLength(32).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ClassName).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Section).HasMaxLength(16).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
