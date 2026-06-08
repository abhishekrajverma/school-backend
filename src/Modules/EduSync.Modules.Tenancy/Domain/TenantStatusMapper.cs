namespace EduSync.Modules.Tenancy.Domain;

public static class TenantStatusMapper
{
    public static string ToApiStatus(TenantStatus status) => status switch
    {
        TenantStatus.Active => "live",
        TenantStatus.Provisioning => "pending",
        TenantStatus.Suspended => "suspended",
        _ => "pending",
    };
}
