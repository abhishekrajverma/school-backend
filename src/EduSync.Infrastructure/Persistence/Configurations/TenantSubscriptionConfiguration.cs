using EduSync.Modules.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions", "tenancy");
        builder.HasKey(x => x.TenantId);
        builder.Property(x => x.PlanId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FeatureFlagsJson).HasColumnType("nvarchar(max)");
    }
}
