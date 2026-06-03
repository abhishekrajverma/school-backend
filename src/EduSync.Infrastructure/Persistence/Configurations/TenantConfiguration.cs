using EduSync.Modules.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants", "tenancy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.ExternalId).IsUnique();
        builder.Property(x => x.Slug).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.LogoUrl).HasMaxLength(1024);
        builder.HasOne(x => x.Subscription).WithOne(x => x.Tenant).HasForeignKey<TenantSubscription>(x => x.TenantId);
    }
}
