using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Assignments.Application;
using EduSync.Modules.Assignments.Domain;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Assignments;

internal static class AssignmentMapping
{
    public static AssignmentDto ToDto(Assignment a) => new(
        a.ExternalId,
        a.Title,
        a.Description,
        a.ClassName,
        a.Section,
        a.Subject,
        a.DueDate.ToString("yyyy-MM-dd"),
        a.Status,
        a.AcademicYearId.ToString());

    public static StudentAssignmentDto ToStudentDto(StudentAssignment sa, Assignment a) => new(
        sa.ExternalId,
        a.ExternalId,
        a.Title,
        a.DueDate.ToString("yyyy-MM-dd"),
        sa.Status,
        sa.SubmissionText,
        sa.Score,
        sa.SubmittedAt);
}

public sealed class ListAssignmentsQueryHandler(EduSyncDbContext db, IAcademicYearContext academicYear, IBranchContext branch)
    : IRequestHandler<ListAssignmentsQuery, Result<PaginatedList<AssignmentDto>>>
{
    public async Task<Result<PaginatedList<AssignmentDto>>> Handle(ListAssignmentsQuery request, CancellationToken ct)
    {
        var query = db.Assignments.AsNoTracking().Where(a => !a.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.ClassName))
        {
            query = query.Where(a => a.ClassName == request.ClassName);
        }

        if (academicYear.IsResolved)
        {
            query = query.Where(a => a.AcademicYearId == academicYear.AcademicYearId);
        }

        if (branch.IsResolved)
        {
            query = query.Where(a => a.BranchId == branch.BranchId);
        }

        query = query.OrderByDescending(a => a.DueDate);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(AssignmentMapping.ToDto).ToList();
        return Result<PaginatedList<AssignmentDto>>.Success(
            PaginatedList<AssignmentDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class CreateAssignmentCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch,
    IAcademicYearContext academicYear)
    : IRequestHandler<CreateAssignmentCommand, Result<AssignmentDto>>
{
    public async Task<Result<AssignmentDto>> Handle(CreateAssignmentCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue || !branch.BranchId.HasValue || !academicYear.AcademicYearId.HasValue)
        {
            return Result<AssignmentDto>.Failure(Error.Forbidden("Tenant, branch, and academic year are required."));
        }

        if (!DateOnly.TryParse(request.Request.DueDate, out var dueDate))
        {
            return Result<AssignmentDto>.Failure(Error.Validation("Invalid due date."));
        }

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            BranchId = branch.BranchId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            AcademicYearId = academicYear.AcademicYearId.Value,
            Title = request.Request.Title.Trim(),
            Description = request.Request.Description,
            ClassName = request.Request.ClassName.Trim(),
            Section = request.Request.Section,
            Subject = request.Request.Subject,
            DueDate = dueDate,
            Status = AssignmentStatuses.Published,
            TeacherExternalId = request.Request.TeacherExternalId,
        };

        db.Assignments.Add(assignment);

        var enrollments = await db.StudentEnrollments.AsNoTracking()
            .Where(e => e.TenantId == tenant.TenantId
                        && e.BranchId == branch.BranchId
                        && e.AcademicYearId == academicYear.AcademicYearId
                        && e.ClassName == assignment.ClassName
                        && e.EnrollmentStatus == EnrollmentStatuses.Enrolled
                        && !e.IsDeleted)
            .Join(db.Students.AsNoTracking(), e => e.StudentId, s => s.Id, (e, s) => new { e.StudentId, s.ExternalId })
            .ToListAsync(ct);

        foreach (var enrollment in enrollments)
        {
            db.StudentAssignments.Add(new StudentAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.TenantId.Value,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                AssignmentId = assignment.Id,
                StudentId = enrollment.StudentId,
                StudentExternalId = enrollment.ExternalId,
                Status = StudentAssignmentStatuses.Pending,
            });
        }

        await db.SaveChangesAsync(ct);
        return Result<AssignmentDto>.Success(AssignmentMapping.ToDto(assignment));
    }
}

public sealed class ListStudentAssignmentsQueryHandler(
    EduSyncDbContext db,
    ICurrentUserContext user,
    IAcademicYearContext academicYear)
    : IRequestHandler<ListStudentAssignmentsQuery, Result<IReadOnlyList<StudentAssignmentDto>>>
{
    public async Task<Result<IReadOnlyList<StudentAssignmentDto>>> Handle(ListStudentAssignmentsQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
        {
            return Result<IReadOnlyList<StudentAssignmentDto>>.Failure(Error.Forbidden("Authenticated user required."));
        }

        var items = await (
            from sa in db.StudentAssignments.AsNoTracking()
            join a in db.Assignments.AsNoTracking() on sa.AssignmentId equals a.Id
            where sa.StudentExternalId == user.UserExternalId
                  && !sa.IsDeleted
                  && !a.IsDeleted
                  && (!academicYear.IsResolved || a.AcademicYearId == academicYear.AcademicYearId)
            orderby a.DueDate descending
            select AssignmentMapping.ToStudentDto(sa, a)).ToListAsync(ct);

        return Result<IReadOnlyList<StudentAssignmentDto>>.Success(items);
    }
}

public sealed class SubmitStudentAssignmentCommandHandler(EduSyncDbContext db, ICurrentUserContext user)
    : IRequestHandler<SubmitStudentAssignmentCommand, Result<StudentAssignmentDto>>
{
    public async Task<Result<StudentAssignmentDto>> Handle(SubmitStudentAssignmentCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.UserExternalId))
        {
            return Result<StudentAssignmentDto>.Failure(Error.Forbidden("Authenticated user required."));
        }

        var assignment = await db.Assignments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ExternalId == request.AssignmentExternalId && !a.IsDeleted, ct);
        if (assignment is null)
        {
            return Result<StudentAssignmentDto>.Failure(Error.NotFound("Assignment not found."));
        }

        var studentAssignment = await db.StudentAssignments
            .FirstOrDefaultAsync(
                sa => sa.AssignmentId == assignment.Id
                      && sa.StudentExternalId == user.UserExternalId
                      && !sa.IsDeleted,
                ct);
        if (studentAssignment is null)
        {
            return Result<StudentAssignmentDto>.Failure(Error.NotFound("Assignment not assigned to this student."));
        }

        if (studentAssignment.Status != StudentAssignmentStatuses.Pending)
        {
            return Result<StudentAssignmentDto>.Failure(Error.Conflict("Assignment already submitted."));
        }

        studentAssignment.SubmissionText = request.Request.SubmissionText.Trim();
        studentAssignment.Status = StudentAssignmentStatuses.Submitted;
        studentAssignment.SubmittedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Result<StudentAssignmentDto>.Success(AssignmentMapping.ToStudentDto(studentAssignment, assignment));
    }
}
