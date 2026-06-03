using EduSync.Modules.Identity.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Identity.Application.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;
