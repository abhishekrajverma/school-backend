using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Security;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Events.Domain;
using EduSync.Modules.Students.Application.Commands;
using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Application.Queries;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Students;

public sealed class ListStudentsQueryHandler(
    IReadDbContextFactory dbFactory,
    IFieldEncryptionService encryption,
    IAcademicYearContext academicYear,
    IBranchContext branch)
    : IRequestHandler<ListStudentsQuery, Result<PaginatedList<StudentDto>>>
{
    public async Task<Result<PaginatedList<StudentDto>>> Handle(
        ListStudentsQuery request,
        CancellationToken cancellationToken)
    {
        await using var db = dbFactory.CreateDbContext();
        var students = db.Students.AsNoTracking().Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.Trim();
            if (term.Length >= 2)
            {
                var pattern = $"%{term}%";
                students = students.Where(s =>
                    EF.Functions.Like(s.FirstName, pattern) ||
                    EF.Functions.Like(s.LastName, pattern) ||
                    EF.Functions.Like(s.Email, pattern) ||
                    EF.Functions.Like(s.AdmissionNo, pattern));
            }
        }

        students = ApplySorting(students, request.Pagination);
        var page = await QueryPagination.ToPaginatedListAsync(students, request.Pagination, cancellationToken);
        var studentIds = page.Items.Select(s => s.Id).ToList();
        var enrollments = await StudentEnrollmentHelper.LoadCurrentEnrollmentsAsync(
            db.StudentEnrollments, studentIds, academicYear, branch, cancellationToken);

        var dtoItems = page.Items.Select(s =>
            StudentSensitiveFields.ToDto(
                s,
                enrollments.GetValueOrDefault(s.Id),
                encryption)).ToList();

        var dtos = PaginatedList<StudentDto>.Create(dtoItems, page.Page, page.PageSize, page.TotalCount);
        return Result<PaginatedList<StudentDto>>.Success(dtos);
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, PaginationQuery pagination)
    {
        var sortBy = pagination.SortBy?.ToLowerInvariant();
        var desc = pagination.IsDescending;

        return sortBy switch
        {
            "name" => desc ? query.OrderByDescending(s => s.LastName).ThenByDescending(s => s.FirstName)
                : query.OrderBy(s => s.LastName).ThenBy(s => s.FirstName),
            _ => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
        };
    }
}

public sealed class GetStudentByIdQueryHandler(
    EduSyncDbContext db,
    IFieldEncryptionService encryption,
    IAcademicYearContext academicYear,
    IBranchContext branch)
    : IRequestHandler<GetStudentByIdQuery, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == request.ExternalId && !s.IsDeleted, cancellationToken);

        if (student is null)
        {
            return Result<StudentDto>.Failure(Error.NotFound("Student not found."));
        }

        var enrollmentQuery = db.StudentEnrollments.AsNoTracking()
            .Where(e => e.StudentId == student.Id && !e.IsDeleted);
        enrollmentQuery = StudentEnrollmentHelper.ActiveEnrollments(enrollmentQuery, academicYear, branch);
        var enrollment = await enrollmentQuery.OrderByDescending(e => e.EnrolledAt).FirstOrDefaultAsync(cancellationToken);

        return Result<StudentDto>.Success(StudentSensitiveFields.ToDto(student, enrollment, encryption));
    }
}

public sealed class CreateStudentCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenantContext,
    IBranchContext branchContext,
    IAcademicYearContext academicYearContext,
    IIntegrationEventCollector events,
    IRegionContext region,
    IHttpContextAccessor httpContextAccessor,
    IFieldEncryptionService encryption)
    : IRequestHandler<CreateStudentCommand, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue || !branchContext.BranchId.HasValue || !academicYearContext.AcademicYearId.HasValue)
        {
            return Result<StudentDto>.Failure(Error.Forbidden("Tenant, branch, and academic year are required."));
        }

        var body = request.Request;
        if (await db.Students.AnyAsync(
                s => s.TenantId == tenantContext.TenantId && s.AdmissionNo == body.AdmissionNo,
                cancellationToken))
        {
            return Result<StudentDto>.Failure(Error.Conflict("Admission number already exists."));
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FirstName = body.FirstName.Trim(),
            LastName = body.LastName.Trim(),
            Email = body.Email.Trim(),
            Phone = body.Phone,
            DateOfBirth = ParseDate(body.DateOfBirth),
            Gender = body.Gender,
            BloodGroup = body.BloodGroup,
            Address = body.Address,
            AdmissionNo = body.AdmissionNo,
            LifecycleStatus = LifecycleStatuses.All.Contains(body.Status) ? body.Status : LifecycleStatuses.Active,
        };

        var enrollment = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            BranchId = branchContext.BranchId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            StudentId = student.Id,
            AcademicYearId = academicYearContext.AcademicYearId.Value,
            ClassName = body.Class,
            Section = body.Section,
            RollNo = body.RollNo,
            EnrollmentStatus = EnrollmentStatuses.Enrolled,
            EnrolledAt = DateTime.UtcNow,
        };

        StudentSensitiveFields.ApplyEncryption(student, encryption);
        db.Students.Add(student);
        db.StudentEnrollments.Add(enrollment);

        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.StudentCreated,
            new { studentExternalId = student.ExternalId, admissionNo = student.AdmissionNo, className = enrollment.ClassName },
            tenantContext,
            region,
            httpContextAccessor));
        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.StudentEnrolled,
            new { studentExternalId = student.ExternalId, enrollmentExternalId = enrollment.ExternalId, className = enrollment.ClassName },
            tenantContext,
            region,
            httpContextAccessor));

        await db.SaveChangesAsync(cancellationToken);
        return Result<StudentDto>.Success(
            StudentSensitiveFields.ToDto(student, enrollment, encryption, body.ParentName, body.ParentPhone, body.ParentEmail));
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, out var date) ? date : null;
}

public sealed class UpdateStudentCommandHandler(
    EduSyncDbContext db,
    IFieldEncryptionService encryption,
    IAcademicYearContext academicYear,
    IBranchContext branch)
    : IRequestHandler<UpdateStudentCommand, Result<StudentDto>>
{
    public async Task<Result<StudentDto>> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await db.Students.FirstOrDefaultAsync(
            s => s.ExternalId == request.ExternalId && !s.IsDeleted,
            cancellationToken);

        if (student is null)
        {
            return Result<StudentDto>.Failure(Error.NotFound("Student not found."));
        }

        var body = request.Request;
        if (body.FirstName is not null) student.FirstName = body.FirstName.Trim();
        if (body.LastName is not null) student.LastName = body.LastName.Trim();
        if (body.Email is not null) student.Email = body.Email.Trim();
        if (body.Phone is not null) student.Phone = body.Phone;
        if (body.DateOfBirth is not null) student.DateOfBirth = DateOnly.TryParse(body.DateOfBirth, out var d) ? d : student.DateOfBirth;
        if (body.Gender is not null) student.Gender = body.Gender;
        if (body.BloodGroup is not null) student.BloodGroup = body.BloodGroup;
        if (body.Address is not null) student.Address = body.Address;
        if (body.AdmissionNo is not null) student.AdmissionNo = body.AdmissionNo;
        if (body.Status is not null)
        {
            var statusResult = student.SetLifecycleStatus(body.Status);
            if (!statusResult.IsSuccess)
            {
                return Result<StudentDto>.Failure(statusResult.Error!);
            }
        }

        var enrollmentQuery = db.StudentEnrollments.Where(e => e.StudentId == student.Id && !e.IsDeleted);
        enrollmentQuery = StudentEnrollmentHelper.ActiveEnrollments(enrollmentQuery, academicYear, branch);
        var enrollment = await enrollmentQuery.OrderByDescending(e => e.EnrolledAt).FirstOrDefaultAsync(cancellationToken);

        if (enrollment is not null)
        {
            if (body.Class is not null) enrollment.ClassName = body.Class;
            if (body.Section is not null) enrollment.Section = body.Section;
            if (body.RollNo is not null) enrollment.RollNo = body.RollNo;
        }

        StudentSensitiveFields.ApplyEncryption(student, encryption);
        await db.SaveChangesAsync(cancellationToken);
        return Result<StudentDto>.Success(
            StudentSensitiveFields.ToDto(student, enrollment, encryption, body.ParentName, body.ParentPhone, body.ParentEmail));
    }
}

public sealed class DeleteStudentCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteStudentCommand, Result>
{
    public async Task<Result> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await db.Students.FirstOrDefaultAsync(
            s => s.ExternalId == request.ExternalId && !s.IsDeleted,
            cancellationToken);

        if (student is null)
        {
            return Result.Failure(Error.NotFound("Student not found."));
        }

        student.IsDeleted = true;
        student.MarkInactive();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
