using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Hostel.Application;

public sealed record HostelRoomDto(
    string Id, string RoomNo, string Block, int Capacity, int Occupied, int Floor,
    string Warden, string Status, decimal MonthlyFee);

public sealed record HostelAllocationDto(
    string Id, string RoomId, string RoomNo, string StudentId, string StudentName,
    string AllocatedOn, string Status);

public sealed record CreateHostelRoomRequest(
    string RoomNo, string Block, int Capacity, int Floor, string Warden, decimal MonthlyFee);

public sealed record CreateAllocationRequest(string RoomId, string StudentId, string StudentName, string AllocatedOn);

public sealed record ListHostelRoomsQuery(PaginationQuery Pagination, string? Block, string? Status)
    : IRequest<Result<PaginatedList<HostelRoomDto>>>;

public sealed record GetHostelRoomByIdQuery(string ExternalId) : IRequest<Result<HostelRoomDto>>;
public sealed record CreateHostelRoomCommand(CreateHostelRoomRequest Request) : IRequest<Result<HostelRoomDto>>;

public sealed record ListHostelAllocationsQuery(PaginationQuery Pagination, string? RoomId)
    : IRequest<Result<PaginatedList<HostelAllocationDto>>>;

public sealed record CreateHostelAllocationCommand(CreateAllocationRequest Request) : IRequest<Result<HostelAllocationDto>>;
