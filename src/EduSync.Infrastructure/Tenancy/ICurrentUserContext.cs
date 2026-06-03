namespace EduSync.Infrastructure.Tenancy;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? UserExternalId { get; }
    string? Role { get; }
}
