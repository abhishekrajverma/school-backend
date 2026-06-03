using EduSync.Modules.Identity.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Identity.Application.Commands;

public sealed record OidcLoginCommand(string IdToken) : IRequest<Result<LoginResponse>>;

public sealed record OidcConfigDto(
    bool Enabled,
    string? Authority,
    string? ClientId,
    string? Scopes);

public sealed record GetOidcConfigQuery : IRequest<OidcConfigDto>;

public sealed record OidcLoginRequest(string IdToken);
