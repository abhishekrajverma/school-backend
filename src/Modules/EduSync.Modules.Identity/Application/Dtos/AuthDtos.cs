namespace EduSync.Modules.Identity.Application.Dtos;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string AccessToken,
    string? RefreshToken,
    int ExpiresIn,
    AuthUserDto User);

public sealed record AuthUserDto(
    string UserId,
    string Name,
    string Email,
    string Role,
    string TenantId,
    IReadOnlyList<string> Permissions);

public sealed record RefreshRequest(string? RefreshToken);
