using EduSync.Infrastructure.Application.Transport;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Portals.Application;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Portals;

public sealed class GetStudentPortalProfileQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetStudentPortalProfileQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetStudentPortalProfileQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == user.UserExternalId && !s.IsDeleted, ct);
        if (student is null) return Result<object>.Failure(Error.NotFound("Student profile not found."));
        return Result<object>.Success(new
        {
            id = student.ExternalId,
            firstName = student.FirstName,
            lastName = student.LastName,
            className = student.ClassName,
            section = student.Section,
            rollNo = student.RollNo,
            email = student.Email,
            attendancePercent = student.AttendancePercent,
            avatarUrl = student.AvatarUrl,
        });
    }
}

public sealed class GetStudentPortalFeesQueryHandler(
    EduSyncDbContext db,
    ICurrentUserContext user,
    IFinancialYearContext financialYear)
    : IRequestHandler<GetStudentPortalFeesQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetStudentPortalFeesQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var query = db.FeeInvoices.AsNoTracking()
            .Where(f => f.StudentExternalId == user.UserExternalId && !f.IsDeleted);
        if (financialYear.IsResolved)
        {
            query = query.Where(f => f.FinancialYear == financialYear.FinancialYear);
        }

        var fees = await query
            .Select(f => new { f.ExternalId, f.InvoiceNo, f.TotalFee, f.Paid, f.Pending, f.Status, f.DueDate })
            .ToListAsync(ct);
        return Result<object>.Success(fees);
    }
}

public sealed class GetStudentPortalAttendanceQueryHandler(
    EduSyncDbContext db,
    ICurrentUserContext user,
    IFinancialYearContext financialYear)
    : IRequestHandler<GetStudentPortalAttendanceQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetStudentPortalAttendanceQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var query = db.AttendanceRecords.AsNoTracking()
            .Where(a => a.EntityExternalId == user.UserExternalId && !a.IsDeleted);
        if (financialYear.IsResolved)
        {
            query = query.Where(a => a.FinancialYear == financialYear.FinancialYear);
        }

        var records = await query
            .OrderByDescending(a => a.Date)
            .Select(a => new { a.ExternalId, a.Date, a.Status, a.CheckIn, a.CheckOut, a.Remarks })
            .Take(60).ToListAsync(ct);
        return Result<object>.Success(records);
    }
}

public sealed class GetStudentPortalExamsQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetStudentPortalExamsQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetStudentPortalExamsQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == user.UserExternalId && !s.IsDeleted, ct);
        if (student is null) return Result<object>.Failure(Error.NotFound("Student not found."));
        var exams = await db.Exams.AsNoTracking()
            .Where(e => e.ClassName == student.ClassName && !e.IsDeleted)
            .Select(e => new { e.ExternalId, e.ExamName, e.Subject, e.Date, e.Status, e.StartTime })
            .ToListAsync(ct);
        return Result<object>.Success(exams);
    }
}

public sealed class GetStudentPortalTimetableQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetStudentPortalTimetableQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetStudentPortalTimetableQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == user.UserExternalId && !s.IsDeleted, ct);
        if (student is null) return Result<object>.Failure(Error.NotFound("Student not found."));
        var entries = await db.TimetableEntries.AsNoTracking()
            .Where(t => t.ClassName == student.ClassName && !t.IsDeleted)
            .Select(t => new { t.ExternalId, t.Day, t.PeriodsJson })
            .ToListAsync(ct);
        return Result<object>.Success(entries);
    }
}

public sealed class GetStudentPortalLibraryQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetStudentPortalLibraryQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetStudentPortalLibraryQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var issues = await db.BookIssues.AsNoTracking()
            .Where(i => i.MemberExternalId == user.UserExternalId && !i.IsDeleted)
            .Select(i => new { i.ExternalId, i.BookTitle, i.IssueDate, i.DueDate, i.Status, i.Fine })
            .ToListAsync(ct);
        return Result<object>.Success(issues);
    }
}

public sealed class GetTeacherPortalProfileQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetTeacherPortalProfileQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetTeacherPortalProfileQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var teacher = await db.Teachers.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalId == user.UserExternalId && !t.IsDeleted, ct);
        if (teacher is null) return Result<object>.Failure(Error.NotFound("Teacher profile not found."));
        return Result<object>.Success(new
        {
            id = teacher.ExternalId,
            firstName = teacher.FirstName,
            lastName = teacher.LastName,
            department = teacher.Department,
            email = teacher.Email,
            phone = teacher.Phone,
        });
    }
}

public sealed class GetTeacherPortalLeavesQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetTeacherPortalLeavesQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetTeacherPortalLeavesQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var leaves = await db.LeaveRequests.AsNoTracking()
            .Where(l => l.EmployeeExternalId == user.UserExternalId && !l.IsDeleted)
            .OrderByDescending(l => l.AppliedOn)
            .Select(l => new { l.ExternalId, l.LeaveType, l.StartDate, l.EndDate, l.Status, l.Days })
            .ToListAsync(ct);
        return Result<object>.Success(leaves);
    }
}

public sealed class GetTeacherPortalPayrollQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetTeacherPortalPayrollQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetTeacherPortalPayrollQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var payroll = await db.PayrollRecords.AsNoTracking()
            .Where(p => p.EmployeeExternalId == user.UserExternalId && !p.IsDeleted)
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .Select(p => new { p.ExternalId, p.Month, p.Year, p.NetSalary, p.Status, p.PaymentDate })
            .ToListAsync(ct);
        return Result<object>.Success(payroll);
    }
}

public sealed class GetTeacherPortalTimetableQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetTeacherPortalTimetableQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetTeacherPortalTimetableQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var teacher = await db.Teachers.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalId == user.UserExternalId && !t.IsDeleted, ct);
        if (teacher is null) return Result<object>.Failure(Error.NotFound("Teacher not found."));
        var name = $"{teacher.FirstName} {teacher.LastName}";
        var entries = await db.TimetableEntries.AsNoTracking()
            .Where(t => !t.IsDeleted && t.PeriodsJson.Contains(name))
            .Select(t => new { t.ClassName, t.Day, t.PeriodsJson })
            .ToListAsync(ct);
        return Result<object>.Success(entries);
    }
}

public sealed class GetParentPortalProfileQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetParentPortalProfileQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetParentPortalProfileQuery request, CancellationToken ct)
    {
        if (user.UserId is null)
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var dbUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.UserId, ct);
        if (dbUser is null) return Result<object>.Failure(Error.NotFound("User not found."));
        var parent = await db.Parents.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == dbUser.Email && !p.IsDeleted, ct)
            ?? await db.Parents.AsNoTracking().FirstOrDefaultAsync(p => p.ExternalId == "1" && !p.IsDeleted, ct);
        if (parent is null) return Result<object>.Failure(Error.NotFound("Parent profile not found."));
        return Result<object>.Success(new
        {
            id = parent.ExternalId,
            firstName = parent.FirstName,
            lastName = parent.LastName,
            email = parent.Email,
            phone = parent.Phone,
        });
    }
}

public sealed class GetParentPortalChildrenQueryHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<GetParentPortalChildrenQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetParentPortalChildrenQuery request, CancellationToken ct)
    {
        if (user.UserId is null)
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var dbUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == user.UserId, ct);
        if (dbUser is null) return Result<object>.Failure(Error.NotFound("User not found."));
        var parent = await db.Parents.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == dbUser.Email && !p.IsDeleted, ct);
        if (parent is null) return Result<object>.Success(Array.Empty<object>());
        var studentIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(parent.StudentIdsJson ?? "[]") ?? [];
        var children = await db.Students.AsNoTracking()
            .Where(s => studentIds.Contains(s.ExternalId) && !s.IsDeleted)
            .Select(s => new { s.ExternalId, s.FirstName, s.LastName, s.ClassName, s.Section, s.RollNo })
            .ToListAsync(ct);
        return Result<object>.Success(children);
    }
}

public sealed class GetParentPortalChildFeesQueryHandler(
    EduSyncDbContext db,
    ICurrentUserContext user,
    IFinancialYearContext financialYear)
    : IRequestHandler<GetParentPortalChildFeesQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetParentPortalChildFeesQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId) && user.UserId is null)
            return Result<object>.Failure(Error.Forbidden("Authenticated user required."));
        var query = db.FeeInvoices.AsNoTracking()
            .Where(f => f.StudentExternalId == request.ChildId && !f.IsDeleted);
        if (financialYear.IsResolved)
        {
            query = query.Where(f => f.FinancialYear == financialYear.FinancialYear);
        }

        var fees = await query
            .Select(f => new { f.ExternalId, f.InvoiceNo, f.TotalFee, f.Paid, f.Pending, f.Status })
            .ToListAsync(ct);
        return Result<object>.Success(fees);
    }
}

public sealed class GetParentPortalChildAttendanceQueryHandler(EduSyncDbContext db, IFinancialYearContext financialYear)
    : IRequestHandler<GetParentPortalChildAttendanceQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetParentPortalChildAttendanceQuery request, CancellationToken ct)
    {
        var query = db.AttendanceRecords.AsNoTracking()
            .Where(a => a.EntityExternalId == request.ChildId && !a.IsDeleted);
        if (financialYear.IsResolved)
        {
            query = query.Where(a => a.FinancialYear == financialYear.FinancialYear);
        }

        var records = await query
            .OrderByDescending(a => a.Date)
            .Select(a => new { a.Date, a.Status, a.Remarks })
            .Take(30).ToListAsync(ct);
        return Result<object>.Success(records);
    }
}

public sealed class GetParentPortalChildTransportQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetParentPortalChildTransportQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetParentPortalChildTransportQuery request, CancellationToken ct)
    {
        var assignment = await db.TransportAssignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.StudentExternalId == request.ChildId && !a.IsDeleted && a.Status == "active", ct);
        if (assignment is null)
            return Result<object>.Success(new { enrolled = false });

        var route = await db.TransportRoutes.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalId == assignment.RouteExternalId && !r.IsDeleted, ct);
        if (route is null)
            return Result<object>.Success(new { enrolled = true, assignment });

        var stops = TransportMapping.ParseStops(route.StopsJson);
        return Result<object>.Success(new
        {
            enrolled = true,
            enrollment = new
            {
                studentId = assignment.StudentExternalId,
                routeId = assignment.RouteExternalId,
                assignment.PickupStopOrder,
                assignment.Shift,
                enrolledSince = assignment.EnrolledSince.ToString("yyyy-MM-dd"),
                assignment.Status,
                assignment.SeatNumber,
            },
            route = new
            {
                route.ExternalId,
                route.RouteName,
                route.VehicleNumber,
                route.DriverName,
                route.MorningTime,
                route.EveningTime,
                stops,
            },
        });
    }
}
