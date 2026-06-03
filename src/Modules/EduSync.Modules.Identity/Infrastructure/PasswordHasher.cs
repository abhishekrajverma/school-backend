using EduSync.Modules.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace EduSync.Modules.Identity.Infrastructure;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string hash) =>
        _hasher.VerifyHashedPassword(new object(), hash, password) != PasswordVerificationResult.Failed;
}
