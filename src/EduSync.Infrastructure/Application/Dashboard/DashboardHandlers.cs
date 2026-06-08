using EduSync.Infrastructure.Caching;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Admissions.Domain;
using EduSync.Modules.Attendance.Domain;
using EduSync.Modules.Dashboard.Application;
using EduSync.Modules.Fees.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Dashboard;

public sealed class GetDashboardQueryHandler(
    IReadDbContextFactory dbFactory,
    ITenantContext tenant,
    IFinancialYearContext financialYear,
    IDashboardCache dashboardCache)
    : IRequestHandler<GetDashboardQuery, Result<DashboardResponseDto>>
{
    public async Task<Result<DashboardResponseDto>> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var fy = financialYear.FinancialYear ?? FinancialYearDefaults.Demo;
        if (tenant.TenantId.HasValue)
        {
            var cached = await dashboardCache.GetAsync(tenant.TenantId.Value, fy, ct);
            if (cached is not null)
            {
                return Result<DashboardResponseDto>.Success(cached);
            }
        }

        await using var db = dbFactory.CreateDbContext();
        var studentsQuery = db.Students.Where(s => !s.IsDeleted && s.FinancialYear == fy);
        var feesQuery = db.FeeInvoices.Where(f => !f.IsDeleted && f.FinancialYear == fy);
        var attendanceQuery = db.AttendanceRecords.Where(a => !a.IsDeleted && a.FinancialYear == fy && a.EntityType == "student");

        var students = await studentsQuery.CountAsync(ct);
        var teachers = await db.Teachers.CountAsync(t => !t.IsDeleted, ct);
        var pendingFees = await feesQuery.Where(f => f.Status != "paid").SumAsync(f => f.Pending, ct);
        var totalCollected = await feesQuery.SumAsync(f => f.Paid, ct);
        var totalOverdue = await feesQuery.Where(f => f.Status == "overdue").SumAsync(f => f.Pending, ct);
        var monthlyRevenue = await db.FeePayments
            .Where(p => !p.IsDeleted && p.PaidAt >= DateTime.UtcNow.AddDays(-30))
            .Join(feesQuery, p => p.FeeInvoiceId, f => f.Id, (p, _) => p.Amount)
            .SumAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayRecords = await attendanceQuery.AsNoTracking()
            .Where(a => a.Date == today).ToListAsync(ct);
        var present = todayRecords.Count(r => r.Status is "present" or "late");
        var attendancePct = todayRecords.Count == 0 ? 0 : Math.Round(100.0 * present / todayRecords.Count, 1);

        var salaryPaid = await db.PayrollRecords.Where(p => !p.IsDeleted && p.Status == "paid")
            .SumAsync(p => p.NetSalary, ct);
        var routes = await db.TransportRoutes.CountAsync(r => !r.IsDeleted && r.Status == "active", ct);
        var admissions = await db.AdmissionApplications.CountAsync(
            a => !a.IsDeleted && a.Status == AdmissionStatuses.Submitted, ct);

        var stats = new DashboardStatsDto(
            students, teachers, pendingFees, monthlyRevenue, attendancePct,
            salaryPaid, routes, admissions);

        var feeByMonth = await db.FeePayments.AsNoTracking()
            .Where(p => !p.IsDeleted && p.PaidAt >= DateTime.UtcNow.AddMonths(-6))
            .Join(feesQuery, p => p.FeeInvoiceId, f => f.Id, (p, f) => new { p.PaidAt, p.Amount, f.Pending })
            .GroupBy(x => new { x.PaidAt.Year, x.PaidAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Collected = g.Sum(x => x.Amount) })
            .ToListAsync(ct);

        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var monthlyFeeCollection = feeByMonth
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .Select(x => new FeeCollectionChartDto(monthNames[x.Month - 1], x.Collected, 0))
            .ToList();

        if (monthlyFeeCollection.Count == 0)
        {
            monthlyFeeCollection =
            [
                new("Jun", 8750000, 480000),
                new("May", 8400000, 560000),
            ];
        }

        var weekStart = today.AddDays(-6);
        var weekRecords = await attendanceQuery.AsNoTracking()
            .Where(a => a.Date >= weekStart && a.Date <= today).ToListAsync(ct);
        var weekPresent = weekRecords.Count(r => r.Status is "present" or "late");
        var weekAvg = weekRecords.Count == 0 ? 0 : Math.Round(100.0 * weekPresent / weekRecords.Count, 1);

        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var monthRecords = await attendanceQuery.AsNoTracking()
            .Where(a => a.Date >= monthStart && a.Date <= today).ToListAsync(ct);
        var monthPresent = monthRecords.Count(r => r.Status is "present" or "late");
        var monthAvg = monthRecords.Count == 0 ? 0 : Math.Round(100.0 * monthPresent / monthRecords.Count, 1);
        var workingDays = monthRecords.Select(r => r.Date).Distinct().Count();

        var studentAttendance = BuildWeeklyAttendanceChart(weekRecords);

        var attendanceSummary = new
        {
            today = new
            {
                present,
                absent = todayRecords.Count - present,
                late = todayRecords.Count(r => r.Status == "late"),
                total = todayRecords.Count,
            },
            thisWeek = new { avgAttendance = weekAvg, improvement = 0.0 },
            thisMonth = new { avgAttendance = monthAvg, workingDays },
        };

        var collectionRate = totalCollected + pendingFees > 0
            ? Math.Round(100.0 * (double)(totalCollected / (totalCollected + pendingFees)), 1)
            : 0.0;

        var feeSummary = new
        {
            totalCollected,
            totalPending = pendingFees,
            totalOverdue,
            collectionRate,
            thisMonth = new { collected = monthlyRevenue, pending = pendingFees },
        };

        var response = new DashboardResponseDto(
            stats, monthlyFeeCollection, studentAttendance, attendanceSummary, feeSummary);
        if (tenant.TenantId.HasValue)
        {
            await dashboardCache.SetAsync(tenant.TenantId.Value, fy, response, ct);
        }

        return Result<DashboardResponseDto>.Success(response);
    }

    private static List<AttendanceChartDto> BuildWeeklyAttendanceChart(IReadOnlyList<AttendanceRecord> weekRecords)
    {
        var weekdays = new[]
        {
            (DayOfWeek.Monday, "Mon"),
            (DayOfWeek.Tuesday, "Tue"),
            (DayOfWeek.Wednesday, "Wed"),
            (DayOfWeek.Thursday, "Thu"),
            (DayOfWeek.Friday, "Fri"),
        };

        return weekdays.Select(pair =>
        {
            var dayRecords = weekRecords.Where(r => r.Date.DayOfWeek == pair.Item1).ToList();
            var present = dayRecords.Count(r => r.Status is "present" or "late");
            return new AttendanceChartDto(pair.Item2, present, dayRecords.Count - present);
        }).ToList();
    }
}

public sealed class GetReportQueryHandler(IReadDbContextFactory dbFactory, IFinancialYearContext financialYear)
    : IRequestHandler<GetReportQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetReportQuery request, CancellationToken ct)
    {
        await using var db = dbFactory.CreateDbContext();
        var fy = financialYear.FinancialYear ?? FinancialYearDefaults.Demo;
        var type = (request.Type ?? "overview").ToLowerInvariant();
        object report = type switch
        {
            "fees" => await db.FeeInvoices.AsNoTracking().Where(f => !f.IsDeleted && f.FinancialYear == fy)
                .Select(f => new { f.ExternalId, f.StudentName, f.ClassName, f.TotalFee, f.Paid, f.Pending, f.Status })
                .Take(100).ToListAsync(ct),
            "attendance" => await db.AttendanceRecords.AsNoTracking().Where(a => !a.IsDeleted && a.FinancialYear == fy)
                .OrderByDescending(a => a.Date).Take(100)
                .Select(a => new { a.ExternalId, a.EntityName, a.ClassName, a.Date, a.Status })
                .ToListAsync(ct),
            "payroll" => await db.PayrollRecords.AsNoTracking().Where(p => !p.IsDeleted)
                .Select(p => new { p.ExternalId, p.EmployeeName, p.Month, p.Year, p.NetSalary, p.Status })
                .Take(100).ToListAsync(ct),
            _ => new { message = "Report type not fully implemented", type },
        };
        return Result<object>.Success(report);
    }
}

public sealed class ExportReportQueryHandler(Reports.IReportExporter exporter)
    : IRequestHandler<ExportReportQuery, Result<ReportExportDto>>
{
    public async Task<Result<ReportExportDto>> Handle(ExportReportQuery request, CancellationToken ct)
    {
        try
        {
            var (content, fileName, contentType) = await exporter.ExportAsync(
                request.Type ?? "fees",
                request.Format ?? "csv",
                ct);
            return Result<ReportExportDto>.Success(new ReportExportDto(content, fileName, contentType));
        }
        catch (ArgumentException ex)
        {
            return Result<ReportExportDto>.Failure(Error.Validation(ex.Message));
        }
    }
}
