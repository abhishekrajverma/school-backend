namespace EduSync.Infrastructure.Security;

public sealed class EncryptionOptions
{
    public bool Enabled { get; set; }
    /// <summary>Base64-encoded 32-byte AES key for field encryption.</summary>
    public string DataKey { get; set; } = string.Empty;
}
