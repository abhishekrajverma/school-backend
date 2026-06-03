using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Identity.Application.Commands;

public sealed record LogoutCommand(Guid UserId, string? RefreshToken) : IRequest<Result>;
