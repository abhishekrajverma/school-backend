namespace EduSync.Infrastructure.Security;

public interface IFieldEncryptionService
{
    bool IsEnabled { get; }
    string Encrypt(string? plainText);
    string? Decrypt(string? stored);
}
