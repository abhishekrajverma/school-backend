using EduSync.Modules.Exams.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("Exams", "exams");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
