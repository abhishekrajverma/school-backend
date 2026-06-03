using EduSync.Modules.Hostel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduSync.Infrastructure.Persistence.Configurations;

public sealed class HostelRoomConfiguration : IEntityTypeConfiguration<HostelRoom>
{
    public void Configure(EntityTypeBuilder<HostelRoom> builder)
    {
        builder.ToTable("Rooms", "hostel");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.Property(x => x.MonthlyFee).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}

public sealed class HostelAllocationConfiguration : IEntityTypeConfiguration<HostelAllocation>
{
    public void Configure(EntityTypeBuilder<HostelAllocation> builder)
    {
        builder.ToTable("Allocations", "hostel");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(x => x.RowVersion).IsRowVersion();
    }
}
