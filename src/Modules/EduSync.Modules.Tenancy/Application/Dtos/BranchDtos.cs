using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Tenancy.Application.Dtos;

public sealed record BranchDto(
    string Id,
    string Code,
    string Name,
    string? Address,
    bool IsHeadOffice,
    bool IsActive);

public sealed record CreateBranchRequest(string Code, string Name, string? Address, bool IsHeadOffice = false);
public sealed record UpdateBranchRequest(string? Name, string? Address, bool? IsActive);

public sealed record ListBranchesQuery : IRequest<Result<IReadOnlyList<BranchDto>>>;
public sealed record GetBranchByIdQuery(string ExternalId) : IRequest<Result<BranchDto>>;
public sealed record CreateBranchCommand(CreateBranchRequest Request) : IRequest<Result<BranchDto>>;
public sealed record UpdateBranchCommand(string ExternalId, UpdateBranchRequest Request) : IRequest<Result<BranchDto>>;
