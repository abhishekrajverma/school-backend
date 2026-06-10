using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Tenancy.Application.Dtos;

public sealed record BranchMembershipDto(
    string UserId,
    string BranchId,
    string Role,
    bool IsActive,
    DateTime JoinedAt);

public sealed record AssignBranchMembershipRequest(string UserEmail, string Role);
public sealed record ListBranchMembershipsQuery(string BranchExternalId) : IRequest<Result<IReadOnlyList<BranchMembershipDto>>>;
public sealed record AssignBranchMembershipCommand(string BranchExternalId, AssignBranchMembershipRequest Request)
    : IRequest<Result<BranchMembershipDto>>;
public sealed record RemoveBranchMembershipCommand(string BranchExternalId, Guid UserId) : IRequest<Result>;
