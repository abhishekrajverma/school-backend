using EduSync.Modules.Students.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class PromotionBatchConfiguration : IEntityTypeConfiguration<PromotionBatch>
{
    public void Configure(EntityTypeBuilder<PromotionBatch> builder)
    {
        builder.ToTable("PromotionBatches", "students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.FromAcademicYearId, x.ToAcademicYearId });
    }
}

internal sealed class PromotionBatchItemConfiguration : IEntityTypeConfiguration<PromotionBatchItem>
{
    public void Configure(EntityTypeBuilder<PromotionBatchItem> builder)
    {
        builder.ToTable("PromotionBatchItems", "students");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Outcome).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.PromotionBatchId, x.StudentId }).IsUnique();
        builder.HasOne(x => x.Batch).WithMany(b => b.Items).HasForeignKey(x => x.PromotionBatchId);
    }
}
