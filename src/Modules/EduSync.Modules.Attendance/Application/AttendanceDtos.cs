using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Attendance.Application;

public sealed record AttendanceRecordDto(
    string Id,
    string EntityType,
    string EntityId,
    string Name,
    string? Class,
    string Date,
    string Status,
    string? CheckIn,
    string? CheckOut,
    string? Remarks);

public sealed record MarkAttendanceRequest(
    string EntityType,
    string EntityId,
    string Name,
    string? Class,
    string Date,
    string Status,
    string? CheckIn,
    string? CheckOut,
    string? Remarks);

public sealed record BulkMarkAttendanceRequest(
    string Date,
    string? Class,
    IReadOnlyList<MarkAttendanceRequest> Records);

public sealed record ListAttendanceQuery(
    PaginationQuery Pagination,
    string? Date,
    string? EntityType,
    string? ClassName) : IRequest<Result<PaginatedList<AttendanceRecordDto>>>;

public sealed record GetAttendanceByIdQuery(string ExternalId) : IRequest<Result<AttendanceRecordDto>>;
public sealed record MarkAttendanceCommand(MarkAttendanceRequest Request) : IRequest<Result<AttendanceRecordDto>>;
public sealed record BulkMarkAttendanceCommand(BulkMarkAttendanceRequest Request) : IRequest<Result<IReadOnlyList<AttendanceRecordDto>>>;
public sealed record GetStudentAttendanceQuery(string StudentId, DateOnly? From, DateOnly? To)
    : IRequest<Result<IReadOnlyList<AttendanceRecordDto>>>;
