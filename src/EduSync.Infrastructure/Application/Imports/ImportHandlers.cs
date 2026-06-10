using System.Globalization;
using System.Text;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Imports.Application;
using EduSync.Modules.Identity.Domain;
using EduSync.Modules.Staff.Domain;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Imports;

internal static class CsvImportHelper
{
    public static List<Dictionary<string, string>> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine)) return [];

        var headers = SplitCsvLine(headerLine).Select(h => h.Trim()).ToArray();
        var rows = new List<Dictionary<string, string>>();
        string? line;
        var lineNo = 1;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = SplitCsvLine(line);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length && i < values.Count; i++)
                dict[headers[i]] = values[i].Trim();
            dict["_line"] = lineNo.ToString();
            rows.Add(dict);
        }
        return rows;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;
        foreach (var c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(c);
        }
        result.Add(current.ToString());
        return result;
    }

    public static string Get(Dictionary<string, string> row, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
        }
        return string.Empty;
    }
}

public sealed class ImportStudentsCsvCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch,
    IAcademicYearContext academicYear)
    : IRequestHandler<ImportStudentsCsvCommand, Result<ImportResultDto>>
{
    public async Task<Result<ImportResultDto>> Handle(ImportStudentsCsvCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue || !branch.BranchId.HasValue || !academicYear.AcademicYearId.HasValue)
        {
            return Result<ImportResultDto>.Failure(Error.Forbidden("Tenant, branch, and academic year are required."));
        }
        var rows = CsvImportHelper.Parse(request.CsvStream);
        var imported = 0;
        var skipped = 0;
        var errors = new List<ImportRowError>();

        foreach (var row in rows)
        {
            var line = int.Parse(row["_line"]);
            var firstName = CsvImportHelper.Get(row, "FirstName", "first_name");
            var lastName = CsvImportHelper.Get(row, "LastName", "last_name");
            var email = CsvImportHelper.Get(row, "Email", "email");
            var admissionNo = CsvImportHelper.Get(row, "AdmissionNo", "admission_no");
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(admissionNo))
            {
                errors.Add(new ImportRowError(line, "FirstName, LastName, and AdmissionNo are required."));
                continue;
            }

            if (await db.Students.AnyAsync(s => s.AdmissionNo == admissionNo && !s.IsDeleted, ct))
            {
                skipped++;
                continue;
            }

            var studentId = Guid.NewGuid();
            db.Students.Add(new Student
            {
                Id = studentId,
                TenantId = tenant.TenantId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                FirstName = firstName,
                LastName = lastName,
                Email = string.IsNullOrWhiteSpace(email) ? $"{admissionNo}@import.local" : email,
                Phone = CsvImportHelper.Get(row, "Phone", "phone"),
                AdmissionNo = admissionNo,
                LifecycleStatus = LifecycleStatuses.Active,
            });
            db.StudentEnrollments.Add(new StudentEnrollment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                BranchId = branch.BranchId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                StudentId = studentId,
                AcademicYearId = academicYear.AcademicYearId.Value,
                ClassName = CsvImportHelper.Get(row, "Class", "class") is { Length: > 0 } c ? c : "N/A",
                Section = CsvImportHelper.Get(row, "Section", "section") is { Length: > 0 } s ? s : "A",
                RollNo = CsvImportHelper.Get(row, "RollNo", "roll_no") is { Length: > 0 } r ? r : admissionNo,
                EnrollmentStatus = EnrollmentStatuses.Enrolled,
                EnrolledAt = DateTime.UtcNow,
            });
            imported++;
        }

        if (imported > 0) await db.SaveChangesAsync(ct);
        return Result<ImportResultDto>.Success(new ImportResultDto(imported, skipped, errors.Count, errors));
    }
}

public sealed class ImportTeachersCsvCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<ImportTeachersCsvCommand, Result<ImportResultDto>>
{
    public async Task<Result<ImportResultDto>> Handle(ImportTeachersCsvCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<ImportResultDto>.Failure(Error.Forbidden("Tenant required."));
        var rows = CsvImportHelper.Parse(request.CsvStream);
        var imported = 0;
        var skipped = 0;
        var errors = new List<ImportRowError>();

        foreach (var row in rows)
        {
            var line = int.Parse(row["_line"]);
            var firstName = CsvImportHelper.Get(row, "FirstName", "first_name");
            var lastName = CsvImportHelper.Get(row, "LastName", "last_name");
            var email = CsvImportHelper.Get(row, "Email", "email");
            var employeeId = CsvImportHelper.Get(row, "EmployeeId", "employee_id");
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(employeeId))
            {
                errors.Add(new ImportRowError(line, "FirstName, LastName, and EmployeeId are required."));
                continue;
            }

            if (await db.Teachers.AnyAsync(t => t.EmployeeId == employeeId && !t.IsDeleted, ct))
            {
                skipped++;
                continue;
            }

            decimal.TryParse(CsvImportHelper.Get(row, "Salary", "salary"), NumberStyles.Any, CultureInfo.InvariantCulture, out var salary);
            db.Teachers.Add(new Teacher
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                FirstName = firstName,
                LastName = lastName,
                Email = string.IsNullOrWhiteSpace(email) ? $"{employeeId}@import.local" : email,
                Phone = CsvImportHelper.Get(row, "Phone", "phone"),
                EmployeeId = employeeId,
                Department = CsvImportHelper.Get(row, "Department", "department") is { Length: > 0 } d ? d : "General",
                Subject = CsvImportHelper.Get(row, "Subject", "subject") is { Length: > 0 } sub ? sub : "General",
                Qualification = CsvImportHelper.Get(row, "Qualification", "qualification"),
                ExperienceYears = int.TryParse(CsvImportHelper.Get(row, "Experience", "experience"), out var exp) ? exp : 0,
                Salary = salary,
                JoiningDate = DateOnly.FromDateTime(DateTime.UtcNow),
                LifecycleStatus = "active",
                ClassesJson = "[]",
            });
            imported++;
        }

        if (imported > 0) await db.SaveChangesAsync(ct);
        return Result<ImportResultDto>.Success(new ImportResultDto(imported, skipped, errors.Count, errors));
    }
}

public sealed class QueueImportStudentsCommandHandler(
    ITenantContext tenant,
    EduSyncDbContext db) : IRequestHandler<QueueImportStudentsCommand, Result<string>>
{
    public async Task<Result<string>> Handle(QueueImportStudentsCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<string>.Failure(Error.Forbidden("Tenant required."));

        var file = await db.StoredFiles.AsNoTracking()
            .FirstOrDefaultAsync(f => f.ExternalId == request.StoredFileExternalId && !f.IsDeleted, ct);
        if (file is null) return Result<string>.Failure(Error.NotFound("Upload file not found."));

        var tenantEntity = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenant.TenantId, ct);
        var jobId = global::Hangfire.BackgroundJob.Enqueue<Infrastructure.Hangfire.HangfireBulkImportJob>(j =>
            j.ImportStudentsAsync(tenantEntity.Id, tenantEntity.Slug, tenantEntity.ExternalId, request.StoredFileExternalId));
        return Result<string>.Success(jobId);
    }
}

public sealed class QueueImportTeachersCommandHandler(
    ITenantContext tenant,
    EduSyncDbContext db) : IRequestHandler<QueueImportTeachersCommand, Result<string>>
{
    public async Task<Result<string>> Handle(QueueImportTeachersCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<string>.Failure(Error.Forbidden("Tenant required."));

        var file = await db.StoredFiles.AsNoTracking()
            .FirstOrDefaultAsync(f => f.ExternalId == request.StoredFileExternalId && !f.IsDeleted, ct);
        if (file is null) return Result<string>.Failure(Error.NotFound("Upload file not found."));

        var tenantEntity = await db.Tenants.AsNoTracking().FirstAsync(t => t.Id == tenant.TenantId, ct);
        var jobId = global::Hangfire.BackgroundJob.Enqueue<Infrastructure.Hangfire.HangfireBulkImportJob>(j =>
            j.ImportTeachersAsync(tenantEntity.Id, tenantEntity.Slug, tenantEntity.ExternalId, request.StoredFileExternalId));
        return Result<string>.Success(jobId);
    }
}
