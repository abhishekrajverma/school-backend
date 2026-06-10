using EduSync.Modules.Exams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> builder)
    {
        builder.ToTable("ExamResults", "exams");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ExamExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StudentExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Grade).HasMaxLength(16);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MarksObtained).HasPrecision(8, 2);
        builder.Property(x => x.TotalMarks).HasPrecision(8, 2);
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.ExamExternalId, x.StudentExternalId, x.AcademicYearId }).IsUnique();
    }
}
