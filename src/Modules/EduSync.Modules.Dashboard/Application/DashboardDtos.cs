using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Dashboard.Application;

public sealed record DashboardStatsDto(
    int TotalStudents,
    int TotalTeachers,
    decimal PendingFees,
    decimal MonthlyRevenue,
    double AttendancePercentage,
    decimal SalaryPaid,
    int TransportRoutes,
    int NewAdmissions);

public sealed record ChartPointDto(string Label, decimal Value);
public sealed record FeeCollectionChartDto(string Month, decimal Collected, decimal Pending);
public sealed record AttendanceChartDto(string Day, int Present, int Absent);

public sealed record DashboardResponseDto(
    DashboardStatsDto Stats,
    IReadOnlyList<FeeCollectionChartDto> MonthlyFeeCollection,
    IReadOnlyList<AttendanceChartDto> StudentAttendance,
    object? AttendanceSummary,
    object? FeeSummary);

public sealed record GetDashboardQuery : IRequest<Result<DashboardResponseDto>>;

public sealed record GetReportQuery(string Type, string? From, string? To) : IRequest<Result<object>>;

public sealed record ExportReportQuery(string Format, string Type, string? From, string? To)
    : IRequest<Result<ReportExportDto>>;

public sealed record ReportExportDto(byte[] Content, string FileName, string ContentType);
