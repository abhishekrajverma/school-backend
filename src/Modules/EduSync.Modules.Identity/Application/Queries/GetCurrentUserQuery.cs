using EduSync.Modules.Identity.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Identity.Application.Queries;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<AuthUserDto>>;
