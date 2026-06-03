using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduSync.Modules.Identity.Application.Abstractions;
using EduSync.Modules.Identity.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EduSync.Modules.Identity.Infrastructure;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string AccessToken, int ExpiresInSeconds) CreateAccessToken(User user, Guid tenantId, string tenantExternalId, string tenantRole)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var expiresMinutes = int.Parse(jwt["AccessTokenMinutes"] ?? "480");
        var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, tenantRole),
            new("tenant_id", tenantId.ToString()),
            new("tenant_external_id", tenantExternalId),
            new("user_external_id", user.ExternalId),
        };

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresMinutes * 60);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

    public string HashToken(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
