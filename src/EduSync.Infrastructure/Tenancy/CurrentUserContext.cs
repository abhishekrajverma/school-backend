using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EduSync.Infrastructure.Tenancy;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue("sub");
            return sub is not null && Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? UserExternalId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("user_external_id");

    public string? Role =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}
