using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Students;

public static class StudentEnrollmentHelper
{
    public static IQueryable<StudentEnrollment> ActiveEnrollments(
        IQueryable<StudentEnrollment> query,
        IAcademicYearContext academicYear,
        IBranchContext branch)
    {
        query = query.Where(e => e.EnrollmentStatus == EnrollmentStatuses.Enrolled);
        if (academicYear.IsResolved)
        {
            query = query.Where(e => e.AcademicYearId == academicYear.AcademicYearId);
        }

        if (branch.IsResolved)
        {
            query = query.Where(e => e.BranchId == branch.BranchId);
        }

        return query;
    }

    public static async Task<Dictionary<Guid, StudentEnrollment>> LoadCurrentEnrollmentsAsync(
        DbSet<StudentEnrollment> enrollments,
        IEnumerable<Guid> studentIds,
        IAcademicYearContext academicYear,
        IBranchContext branch,
        CancellationToken ct)
    {
        var ids = studentIds.ToList();
        if (ids.Count == 0)
        {
            return [];
        }

        var query = enrollments.AsNoTracking().Where(e => ids.Contains(e.StudentId) && !e.IsDeleted);
        query = ActiveEnrollments(query, academicYear, branch);
        var list = await query.ToListAsync(ct);
        return list.GroupBy(e => e.StudentId).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EnrolledAt).First());
    }
}
