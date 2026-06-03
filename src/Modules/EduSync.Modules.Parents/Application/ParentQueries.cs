using EduSync.Modules.Parents.Application.Dtos;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Parents.Application;

public sealed record ListParentsQuery(PaginationQuery Pagination) : IRequest<Result<PaginatedList<ParentDto>>>;
public sealed record GetParentByIdQuery(string ExternalId) : IRequest<Result<ParentDto>>;
public sealed record CreateParentCommand(CreateParentRequest Request) : IRequest<Result<ParentDto>>;
public sealed record UpdateParentCommand(string ExternalId, UpdateParentRequest Request) : IRequest<Result<ParentDto>>;
public sealed record DeleteParentCommand(string ExternalId) : IRequest<Result>;
