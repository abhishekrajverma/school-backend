using EduSync.Modules.Students.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("Enrollments", "students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ClassName).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Section).HasMaxLength(16).IsRequired();
        builder.Property(x => x.RollNo).HasMaxLength(32).IsRequired();
        builder.Property(x => x.EnrollmentStatus).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.AcademicYearId, x.StudentId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.AcademicYearId, x.EnrollmentStatus });
        builder.HasOne(x => x.Student).WithMany(s => s.Enrollments).HasForeignKey(x => x.StudentId);
    }
}
