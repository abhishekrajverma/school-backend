using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;

namespace EduSync.Infrastructure.Tenancy;

public static class TenantLoginGuard
{
    public static Result? ValidateForLogin(Tenant? tenant)
    {
        if (tenant is null)
        {
            return Result.Failure(Error.Forbidden("Tenant is not available."));
        }

        return tenant.Status switch
        {
            TenantStatus.Active => null,
            TenantStatus.Provisioning => Result.Failure(Error.Forbidden("School not active yet.")),
            TenantStatus.Suspended => Result.Failure(Error.Forbidden("School suspended.")),
            _ => Result.Failure(Error.Forbidden("Tenant is not available.")),
        };
    }
}
