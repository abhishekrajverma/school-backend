using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace EduSync.Infrastructure.Security;

public interface IOidcTokenValidator
{
    Task<ClaimsPrincipal?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed class OidcTokenValidator(IOptions<OidcOptions> options) : IOidcTokenValidator
{
    public async Task<ClaimsPrincipal?> ValidateIdTokenAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var oidc = options.Value;
        if (!oidc.Enabled || string.IsNullOrWhiteSpace(oidc.Authority))
        {
            return null;
        }

        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"{oidc.Authority.TrimEnd('/')}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());

        var oidcConfig = await configManager.GetConfigurationAsync(cancellationToken);
        var validation = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = oidcConfig.Issuer,
            ValidateAudience = true,
            ValidAudience = oidc.ClientId,
            ValidateLifetime = true,
            IssuerSigningKeys = oidcConfig.SigningKeys,
            ClockSkew = TimeSpan.FromMinutes(2),
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(idToken, validation, out _);
    }
}
