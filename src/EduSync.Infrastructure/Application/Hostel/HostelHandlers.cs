using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Hostel.Application;
using EduSync.Modules.Hostel.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Hostel;

internal static class HostelMapping
{
    public static HostelRoomDto ToDto(HostelRoom r) => new(
        r.ExternalId, r.RoomNo, r.Block, r.Capacity, r.Occupied, r.Floor,
        r.Warden, r.Status, r.MonthlyFee);

    public static HostelAllocationDto ToDto(HostelAllocation a) => new(
        a.ExternalId, a.RoomExternalId, a.Room.RoomNo, a.StudentExternalId, a.StudentName,
        a.AllocatedOn.ToString("yyyy-MM-dd"), a.Status);
}

public sealed class ListHostelRoomsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListHostelRoomsQuery, Result<PaginatedList<HostelRoomDto>>>
{
    public async Task<Result<PaginatedList<HostelRoomDto>>> Handle(ListHostelRoomsQuery request, CancellationToken ct)
    {
        var query = db.HostelRooms.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Block)) query = query.Where(x => x.Block == request.Block);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        query = query.OrderBy(x => x.Block).ThenBy(x => x.RoomNo);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(HostelMapping.ToDto).ToList();
        return Result<PaginatedList<HostelRoomDto>>.Success(
            PaginatedList<HostelRoomDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetHostelRoomByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetHostelRoomByIdQuery, Result<HostelRoomDto>>
{
    public async Task<Result<HostelRoomDto>> Handle(GetHostelRoomByIdQuery request, CancellationToken ct)
    {
        var r = await db.HostelRooms.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return r is null ? Result<HostelRoomDto>.Failure(Error.NotFound("Room not found."))
            : Result<HostelRoomDto>.Success(HostelMapping.ToDto(r));
    }
}

public sealed class CreateHostelRoomCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateHostelRoomCommand, Result<HostelRoomDto>>
{
    public async Task<Result<HostelRoomDto>> Handle(CreateHostelRoomCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<HostelRoomDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        var room = new HostelRoom
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            RoomNo = b.RoomNo, Block = b.Block, Capacity = b.Capacity, Occupied = 0,
            Floor = b.Floor, Warden = b.Warden, MonthlyFee = b.MonthlyFee, Status = "available",
        };
        db.HostelRooms.Add(room);
        await db.SaveChangesAsync(ct);
        return Result<HostelRoomDto>.Success(HostelMapping.ToDto(room));
    }
}

public sealed class UpdateHostelRoomCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateHostelRoomCommand, Result<HostelRoomDto>>
{
    public async Task<Result<HostelRoomDto>> Handle(UpdateHostelRoomCommand request, CancellationToken ct)
    {
        var room = await db.HostelRooms.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (room is null) return Result<HostelRoomDto>.Failure(Error.NotFound("Room not found."));
        var b = request.Request;
        if (b.RoomNo is not null) room.RoomNo = b.RoomNo;
        if (b.Block is not null) room.Block = b.Block;
        if (b.Capacity.HasValue) room.Capacity = b.Capacity.Value;
        if (b.Floor.HasValue) room.Floor = b.Floor.Value;
        if (b.Warden is not null) room.Warden = b.Warden;
        if (b.MonthlyFee.HasValue) room.MonthlyFee = b.MonthlyFee.Value;
        if (b.Status is not null) room.Status = b.Status;
        await db.SaveChangesAsync(ct);
        return Result<HostelRoomDto>.Success(HostelMapping.ToDto(room));
    }
}

public sealed class DeleteHostelRoomCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteHostelRoomCommand, Result>
{
    public async Task<Result> Handle(DeleteHostelRoomCommand request, CancellationToken ct)
    {
        var room = await db.HostelRooms.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (room is null) return Result.Failure(Error.NotFound("Room not found."));
        room.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public sealed class ListHostelAllocationsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListHostelAllocationsQuery, Result<PaginatedList<HostelAllocationDto>>>
{
    public async Task<Result<PaginatedList<HostelAllocationDto>>> Handle(ListHostelAllocationsQuery request, CancellationToken ct)
    {
        var query = db.HostelAllocations.AsNoTracking().Include(a => a.Room).Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.RoomId)) query = query.Where(x => x.RoomExternalId == request.RoomId);
        query = query.OrderByDescending(x => x.AllocatedOn);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(HostelMapping.ToDto).ToList();
        return Result<PaginatedList<HostelAllocationDto>>.Success(
            PaginatedList<HostelAllocationDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class CreateHostelAllocationCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateHostelAllocationCommand, Result<HostelAllocationDto>>
{
    public async Task<Result<HostelAllocationDto>> Handle(CreateHostelAllocationCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<HostelAllocationDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        if (!DateOnly.TryParse(b.AllocatedOn, out var allocatedOn))
            return Result<HostelAllocationDto>.Failure(Error.Validation("Invalid allocation date."));
        var room = await db.HostelRooms.FirstOrDefaultAsync(x => x.ExternalId == b.RoomId && !x.IsDeleted, ct);
        if (room is null) return Result<HostelAllocationDto>.Failure(Error.NotFound("Room not found."));
        if (room.Occupied >= room.Capacity)
            return Result<HostelAllocationDto>.Failure(Error.Validation("Room is full."));
        room.Occupied++;
        room.Status = room.Occupied >= room.Capacity ? "full" : "available";
        var allocation = new HostelAllocation
        {
            Id = Guid.NewGuid(), TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            RoomId = room.Id, RoomExternalId = room.ExternalId,
            StudentExternalId = b.StudentId, StudentName = b.StudentName,
            AllocatedOn = allocatedOn, Status = "active", Room = room,
        };
        db.HostelAllocations.Add(allocation);
        await db.SaveChangesAsync(ct);
        return Result<HostelAllocationDto>.Success(HostelMapping.ToDto(allocation));
    }
}
