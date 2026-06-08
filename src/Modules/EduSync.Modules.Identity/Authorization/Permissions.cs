namespace EduSync.Modules.Identity.Authorization;

/// <summary>
/// Fine-grained permission identifiers used as ASP.NET authorization policy names.
/// </summary>
public static class Permissions
{
    public const string StudentsRead = "students.read";
    public const string StudentsWrite = "students.write";
    public const string StudentsDelete = "students.delete";

    public const string TeachersRead = "teachers.read";
    public const string TeachersWrite = "teachers.write";
    public const string TeachersDelete = "teachers.delete";

    public const string ParentsRead = "parents.read";
    public const string ParentsWrite = "parents.write";
    public const string ParentsDelete = "parents.delete";

    public const string AcademicsRead = "academics.read";
    public const string AcademicsWrite = "academics.write";

    public const string AdmissionsRead = "admissions.read";
    public const string AdmissionsManage = "admissions.manage";

    public const string AttendanceRead = "attendance.read";
    public const string AttendanceWrite = "attendance.write";

    public const string FeesRead = "fees.read";
    public const string FeesWrite = "fees.write";
    public const string PaymentsRead = "payments.read";

    public const string ExamsRead = "exams.read";
    public const string ExamsWrite = "exams.write";

    public const string TimetableRead = "timetable.read";
    public const string TimetableWrite = "timetable.write";

    public const string NotificationsRead = "notifications.read";
    public const string NotificationsWrite = "notifications.write";

    public const string PayrollRead = "payroll.read";
    public const string PayrollWrite = "payroll.write";
    public const string PayrollProcess = "payroll.process";

    public const string LeaveRead = "leave.read";
    public const string LeaveWrite = "leave.write";
    public const string LeaveApprove = "leave.approve";

    public const string LibraryRead = "library.read";
    public const string LibraryWrite = "library.write";

    public const string TransportRead = "transport.read";
    public const string TransportWrite = "transport.write";

    public const string HostelRead = "hostel.read";
    public const string HostelWrite = "hostel.write";

    public const string InventoryRead = "inventory.read";
    public const string InventoryWrite = "inventory.write";

    public const string DashboardRead = "dashboard.read";
    public const string ReportsRead = "reports.read";
    public const string ReportsExport = "reports.export";

    public const string UploadsRead = "uploads.read";
    public const string UploadsWrite = "uploads.write";

    public const string JobsRun = "jobs.run";
    public const string ImportsRun = "imports.run";

    public const string EventsRead = "events.read";
    public const string AuditRead = "audit.read";
    public const string WebhooksManage = "webhooks.manage";
    public const string RetentionManage = "retention.manage";
    public const string ChaosRead = "chaos.read";

    public const string TenantsRead = "tenants.read";
    public const string TenantsManage = "tenants.manage";

    public const string CompanyRead = "company.read";
    public const string EnquiriesRead = "enquiries.read";
    public const string EnquiriesManage = "enquiries.manage";

    public const string FinancialYearRead = "financial-year.read";
    public const string FinancialYearWrite = "financial-year.write";

    public const string PortalStudent = "portal.student";
    public const string PortalTeacher = "portal.teacher";
    public const string PortalParent = "portal.parent";

    public static readonly IReadOnlyList<string> All =
    [
        StudentsRead, StudentsWrite, StudentsDelete,
        TeachersRead, TeachersWrite, TeachersDelete,
        ParentsRead, ParentsWrite, ParentsDelete,
        AcademicsRead, AcademicsWrite,
        AdmissionsRead, AdmissionsManage,
        AttendanceRead, AttendanceWrite,
        FeesRead, FeesWrite, PaymentsRead,
        ExamsRead, ExamsWrite,
        TimetableRead, TimetableWrite,
        NotificationsRead, NotificationsWrite,
        PayrollRead, PayrollWrite, PayrollProcess,
        LeaveRead, LeaveWrite, LeaveApprove,
        LibraryRead, LibraryWrite,
        TransportRead, TransportWrite,
        HostelRead, HostelWrite,
        InventoryRead, InventoryWrite,
        DashboardRead, ReportsRead, ReportsExport,
        UploadsRead, UploadsWrite,
        JobsRun, ImportsRun,
        EventsRead, AuditRead, WebhooksManage, RetentionManage, ChaosRead,
        TenantsRead, TenantsManage,
        CompanyRead, EnquiriesRead, EnquiriesManage,
        FinancialYearRead, FinancialYearWrite,
        PortalStudent, PortalTeacher, PortalParent,
    ];

    public static readonly IReadOnlySet<string> AdminOnly =
        new HashSet<string>(StringComparer.Ordinal)
        {
            RetentionManage,
            WebhooksManage,
            ChaosRead,
        };
}
