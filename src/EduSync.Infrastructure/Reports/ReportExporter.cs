using System.Globalization;
using System.Text;
using EduSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace EduSync.Infrastructure.Reports;

public sealed class ReportExporter(IReadDbContextFactory dbFactory) : IReportExporter
{
    public async Task<(byte[] Content, string FileName, string ContentType)> ExportAsync(
        string reportType,
        string format,
        CancellationToken cancellationToken = default)
    {
        await using var db = dbFactory.CreateDbContext();
        var type = reportType.ToLowerInvariant();
        var fmt = format.ToLowerInvariant();
        var date = DateTime.UtcNow.ToString("yyyyMMdd");

        return fmt switch
        {
            "pdf" => type switch
            {
                "fees" => await ExportFeesPdfAsync(db, date, cancellationToken),
                "attendance" => await ExportAttendancePdfAsync(db, date, cancellationToken),
                "payroll" => await ExportPayrollPdfAsync(db, date, cancellationToken),
                _ => throw new ArgumentException($"Unknown report type '{type}'."),
            },
            "csv" => type switch
            {
                "fees" => ExportCsv($"fees-report-{date}.csv", await BuildFeesCsvAsync(db, cancellationToken)),
                "attendance" => ExportCsv($"attendance-report-{date}.csv", await BuildAttendanceCsvAsync(db, cancellationToken)),
                "payroll" => ExportCsv($"payroll-report-{date}.csv", await BuildPayrollCsvAsync(db, cancellationToken)),
                _ => throw new ArgumentException($"Unknown report type '{type}'."),
            },
            _ => throw new ArgumentException($"Unsupported format '{format}'. Use csv or pdf."),
        };
    }

    private static (byte[] Content, string FileName, string ContentType) ExportCsv(string fileName, string csv) =>
        (Encoding.UTF8.GetBytes(csv), fileName, "text/csv");

    private static async Task<(byte[], string, string)> ExportFeesPdfAsync(EduSyncDbContext db, string date, CancellationToken ct)
    {
        var rows = await db.FeeInvoices.AsNoTracking().Where(f => !f.IsDeleted)
            .OrderByDescending(f => f.DueDate).Take(100)
            .Select(f => new[] { f.InvoiceNo, f.StudentName, f.ClassName, f.Pending.ToString("N0"), f.Status })
            .ToListAsync(ct);
        return (BuildTablePdf("Fee Report", ["Invoice", "Student", "Class", "Pending", "Status"], rows),
            $"fees-report-{date}.pdf", "application/pdf");
    }

    private static async Task<(byte[], string, string)> ExportAttendancePdfAsync(EduSyncDbContext db, string date, CancellationToken ct)
    {
        var rows = await db.AttendanceRecords.AsNoTracking().Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.Date).Take(100)
            .Select(a => new[] { a.EntityName, a.ClassName ?? "", a.Date.ToString("yyyy-MM-dd"), a.Status })
            .ToListAsync(ct);
        return (BuildTablePdf("Attendance Report", ["Name", "Class", "Date", "Status"], rows),
            $"attendance-report-{date}.pdf", "application/pdf");
    }

    private static async Task<(byte[], string, string)> ExportPayrollPdfAsync(EduSyncDbContext db, string date, CancellationToken ct)
    {
        var rows = await db.PayrollRecords.AsNoTracking().Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.Year).Take(100)
            .Select(p => new[] { p.EmployeeName, p.Month, p.Year.ToString(), p.NetSalary.ToString("N0"), p.Status })
            .ToListAsync(ct);
        return (BuildTablePdf("Payroll Report", ["Employee", "Month", "Year", "Net", "Status"], rows),
            $"payroll-report-{date}.pdf", "application/pdf");
    }

    private static byte[] BuildTablePdf(string title, string[] headers, List<string[]> rows)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        var titleFont = new XFont("Arial", 14, XFontStyle.Bold);
        var headerFont = new XFont("Arial", 9, XFontStyle.Bold);
        var bodyFont = new XFont("Arial", 8, XFontStyle.Regular);
        double y = 40;
        gfx.DrawString(title, titleFont, XBrushes.DarkBlue, new XRect(40, 20, 500, 20), XStringFormats.TopLeft);
        double x = 40;
        foreach (var h in headers)
        {
            gfx.DrawString(h, headerFont, XBrushes.Black, x, y);
            x += 110;
        }
        y += 18;
        foreach (var row in rows)
        {
            if (y > page.Height - 40) break;
            x = 40;
            foreach (var cell in row)
            {
                var text = cell.Length > 18 ? cell[..18] + "…" : cell;
                gfx.DrawString(text, bodyFont, XBrushes.Black, x, y);
                x += 110;
            }
            y += 14;
        }
        using var ms = new MemoryStream();
        document.Save(ms, false);
        return ms.ToArray();
    }

    private static async Task<string> BuildFeesCsvAsync(EduSyncDbContext db, CancellationToken ct)
    {
        var rows = await db.FeeInvoices.AsNoTracking().Where(f => !f.IsDeleted)
            .OrderByDescending(f => f.DueDate).Take(500)
            .Select(f => new { f.InvoiceNo, f.StudentName, f.ClassName, f.TotalFee, f.Paid, f.Pending, f.Status, f.DueDate })
            .ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("InvoiceNo,StudentName,Class,TotalFee,Paid,Pending,Status,DueDate");
        foreach (var r in rows)
        {
            sb.AppendLine(string.Join(',',
                Csv(r.InvoiceNo), Csv(r.StudentName), Csv(r.ClassName),
                r.TotalFee.ToString(CultureInfo.InvariantCulture),
                r.Paid.ToString(CultureInfo.InvariantCulture),
                r.Pending.ToString(CultureInfo.InvariantCulture),
                Csv(r.Status), r.DueDate.ToString("yyyy-MM-dd")));
        }
        return sb.ToString();
    }

    private static async Task<string> BuildAttendanceCsvAsync(EduSyncDbContext db, CancellationToken ct)
    {
        var rows = await db.AttendanceRecords.AsNoTracking().Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.Date).Take(500)
            .Select(a => new { a.EntityName, a.ClassName, a.Date, a.Status })
            .ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Name,Class,Date,Status");
        foreach (var r in rows)
            sb.AppendLine($"{Csv(r.EntityName)},{Csv(r.ClassName ?? "")},{r.Date:yyyy-MM-dd},{Csv(r.Status)}");
        return sb.ToString();
    }

    private static async Task<string> BuildPayrollCsvAsync(EduSyncDbContext db, CancellationToken ct)
    {
        var rows = await db.PayrollRecords.AsNoTracking().Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).Take(500)
            .Select(p => new { p.EmployeeName, p.Month, p.Year, p.NetSalary, p.Status })
            .ToListAsync(ct);
        var sb = new StringBuilder();
        sb.AppendLine("Employee,Month,Year,NetSalary,Status");
        foreach (var r in rows)
            sb.AppendLine($"{Csv(r.EmployeeName)},{Csv(r.Month)},{r.Year},{r.NetSalary},{Csv(r.Status)}");
        return sb.ToString();
    }

    private static string Csv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
