using EduSync.Modules.Identity.Domain;

namespace EduSync.Modules.Identity.Application.Abstractions;

public interface IJwtTokenService
{
    (string AccessToken, int ExpiresInSeconds) CreateAccessToken(User user, Guid tenantId, string tenantExternalId, string tenantRole);
    string GenerateRefreshToken();
    string HashToken(string token);
}
