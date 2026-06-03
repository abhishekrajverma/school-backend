using EduSync.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        builder.ToTable("TenantMemberships", "identity");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        builder.Property(x => x.Role).HasMaxLength(32).IsRequired();
        builder.HasOne(x => x.User).WithMany(x => x.Memberships).HasForeignKey(x => x.UserId);
    }
}
