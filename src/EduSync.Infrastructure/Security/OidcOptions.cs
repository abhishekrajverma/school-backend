namespace EduSync.Infrastructure.Security;

public sealed class OidcOptions
{
    public bool Enabled { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Scopes { get; set; } = "openid profile email";
}
