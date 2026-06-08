using EduSync.Api.Endpoints;

namespace EduSync.Api.Extensions;

public static class ApiEndpointExtensions
{
    public static void MapEduSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");
        app.MapAuthEndpoints();
        app.MapTenantEndpoints();
        app.MapStudentEndpoints();
        app.MapTeacherEndpoints();
        app.MapParentEndpoints();
        app.MapAcademicsEndpoints();
        app.MapAdmissionEndpoints();
        app.MapAttendanceEndpoints();
        app.MapFeesEndpoints();
        app.MapExamEndpoints();
        app.MapTimetableEndpoints();
        app.MapNotificationEndpoints();
        app.MapPayrollEndpoints();
        app.MapLeaveEndpoints();
        app.MapLibraryEndpoints();
        app.MapTransportEndpoints();
        app.MapHostelEndpoints();
        app.MapInventoryEndpoints();
        app.MapDashboardEndpoints();
        app.MapPortalEndpoints();
        app.MapUploadEndpoints();
        app.MapJobEndpoints();
        app.MapImportEndpoints();
        app.MapEventEndpoints();
        app.MapRegionEndpoints();
        app.MapAuditEndpoints();
        app.MapWebhookEndpoints();
        app.MapChaosEndpoints();
        app.MapVersionEndpoints();
        app.MapRetentionEndpoints();
        app.MapCompanyEndpoints();
        app.MapEnquiryEndpoints();
        app.MapFinancialYearEndpoints();
    }
}
