using EduSync.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

internal sealed class BranchMembershipConfiguration : IEntityTypeConfiguration<BranchMembership>
{
    public void Configure(EntityTypeBuilder<BranchMembership> builder)
    {
        builder.ToTable("BranchMemberships", "identity");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.BranchId, x.UserId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.UserId });
        builder.Property(x => x.Role).HasMaxLength(32).IsRequired();
    }
}
