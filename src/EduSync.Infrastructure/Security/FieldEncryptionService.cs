using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Security;

public sealed class FieldEncryptionService(IOptions<EncryptionOptions> options) : IFieldEncryptionService
{
    private const string Prefix = "enc:v1:";
    private readonly EncryptionOptions _options = options.Value;

    public bool IsEnabled => _options.Enabled && !string.IsNullOrWhiteSpace(_options.DataKey);

    public string Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText) || !IsEnabled)
        {
            return plainText ?? string.Empty;
        }

        if (plainText.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return plainText;
        }

        var key = Convert.FromBase64String(_options.DataKey);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plainBytes, cipher, tag);
        return Prefix + Convert.ToBase64String(nonce) + ":" + Convert.ToBase64String(cipher) + ":" + Convert.ToBase64String(tag);
    }

    public string? Decrypt(string? stored)
    {
        if (string.IsNullOrEmpty(stored))
        {
            return stored;
        }

        if (!stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return stored;
        }

        if (!IsEnabled)
        {
            return "***";
        }

        var payload = stored[Prefix.Length..];
        var parts = payload.Split(':');
        if (parts.Length != 3)
        {
            return stored;
        }

        var nonce = Convert.FromBase64String(parts[0]);
        var cipher = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var key = Convert.FromBase64String(_options.DataKey);
        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }
}
