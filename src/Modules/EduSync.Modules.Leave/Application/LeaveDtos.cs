using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Leave.Application;

public sealed record LeaveProofDocumentDto(string Name, long Size, string Type, long? LastModified, string? PreviewUrl);

public sealed record LeaveRequestDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    string Department,
    string LeaveType,
    string StartDate,
    string EndDate,
    int Days,
    string Reason,
    string Status,
    string AppliedOn,
    string? ApprovedBy,
    string? ApprovedOn,
    LeaveProofDocumentDto? ProofDocument);

public sealed record CreateLeaveRequest(
    string? EmployeeId,
    string? EmployeeName,
    string? Department,
    string LeaveType,
    string StartDate,
    string EndDate,
    string Reason,
    LeaveProofDocumentDto? ProofDocument);

public sealed record ListLeaveRequestsQuery(PaginationQuery Pagination, string? Status, string? EmployeeId)
    : IRequest<Result<PaginatedList<LeaveRequestDto>>>;

public sealed record GetLeaveByIdQuery(string ExternalId) : IRequest<Result<LeaveRequestDto>>;
public sealed record CreateLeaveCommand(CreateLeaveRequest Request) : IRequest<Result<LeaveRequestDto>>;
public sealed record ApproveLeaveCommand(string ExternalId, string? ApprovedBy) : IRequest<Result<LeaveRequestDto>>;
public sealed record RejectLeaveCommand(string ExternalId, string? ApprovedBy) : IRequest<Result<LeaveRequestDto>>;
