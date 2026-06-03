using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Staff.Application;
using EduSync.Modules.Staff.Application.Dtos;
using EduSync.Modules.Staff.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Staff;

public sealed class ListTeachersQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListTeachersQuery, Result<PaginatedList<TeacherDto>>>
{
    public async Task<Result<PaginatedList<TeacherDto>>> Handle(ListTeachersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Teachers.AsNoTracking().Where(t => !t.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(t =>
                t.FirstName.ToLower().Contains(term) ||
                t.LastName.ToLower().Contains(term) ||
                t.Email.ToLower().Contains(term) ||
                t.EmployeeId.ToLower().Contains(term) ||
                t.Department.ToLower().Contains(term));
        }

        query = request.Pagination.IsDescending
            ? query.OrderByDescending(t => t.LastName)
            : query.OrderBy(t => t.LastName);

        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, cancellationToken);
        var items = page.Items.Select(TeacherMapping.ToDto).ToList();
        return Result<PaginatedList<TeacherDto>>.Success(
            PaginatedList<TeacherDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetTeacherByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetTeacherByIdQuery, Result<TeacherDto>>
{
    public async Task<Result<TeacherDto>> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        var teacher = await db.Teachers.AsNoTracking()
            .FirstOrDefaultAsync(t => t.ExternalId == request.ExternalId && !t.IsDeleted, cancellationToken);
        return teacher is null
            ? Result<TeacherDto>.Failure(Error.NotFound("Teacher not found."))
            : Result<TeacherDto>.Success(TeacherMapping.ToDto(teacher));
    }
}

public sealed class CreateTeacherCommandHandler(EduSyncDbContext db, ITenantContext tenantContext)
    : IRequestHandler<CreateTeacherCommand, Result<TeacherDto>>
{
    public async Task<Result<TeacherDto>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return Result<TeacherDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var body = request.Request;
        if (await db.Teachers.AnyAsync(
                t => t.TenantId == tenantContext.TenantId && t.EmployeeId == body.EmployeeId,
                cancellationToken))
        {
            return Result<TeacherDto>.Failure(Error.Conflict("Employee ID already exists."));
        }

        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FirstName = body.FirstName.Trim(),
            LastName = body.LastName.Trim(),
            Email = body.Email.Trim(),
            Phone = body.Phone,
            EmployeeId = body.EmployeeId.Trim(),
            Department = body.Department.Trim(),
            Subject = body.Subject.Trim(),
            Qualification = body.Qualification,
            ExperienceYears = body.Experience,
            Salary = body.Salary,
            JoiningDate = DateOnly.TryParse(body.JoiningDate, out var jd) ? jd : null,
            Status = body.Status,
            ClassesJson = TeacherMapping.SerializeClasses(body.Classes),
        };

        db.Teachers.Add(teacher);
        await db.SaveChangesAsync(cancellationToken);
        return Result<TeacherDto>.Success(TeacherMapping.ToDto(teacher));
    }
}

public sealed class UpdateTeacherCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateTeacherCommand, Result<TeacherDto>>
{
    public async Task<Result<TeacherDto>> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await db.Teachers.FirstOrDefaultAsync(
            t => t.ExternalId == request.ExternalId && !t.IsDeleted, cancellationToken);
        if (teacher is null)
        {
            return Result<TeacherDto>.Failure(Error.NotFound("Teacher not found."));
        }

        var body = request.Request;
        if (body.FirstName is not null) teacher.FirstName = body.FirstName.Trim();
        if (body.LastName is not null) teacher.LastName = body.LastName.Trim();
        if (body.Email is not null) teacher.Email = body.Email.Trim();
        if (body.Phone is not null) teacher.Phone = body.Phone;
        if (body.EmployeeId is not null) teacher.EmployeeId = body.EmployeeId.Trim();
        if (body.Department is not null) teacher.Department = body.Department.Trim();
        if (body.Subject is not null) teacher.Subject = body.Subject.Trim();
        if (body.Qualification is not null) teacher.Qualification = body.Qualification;
        if (body.Experience is not null) teacher.ExperienceYears = body.Experience.Value;
        if (body.Salary is not null) teacher.Salary = body.Salary.Value;
        if (body.JoiningDate is not null && DateOnly.TryParse(body.JoiningDate, out var jd)) teacher.JoiningDate = jd;
        if (body.Status is not null) teacher.Status = body.Status;
        if (body.Classes is not null) teacher.ClassesJson = TeacherMapping.SerializeClasses(body.Classes);

        await db.SaveChangesAsync(cancellationToken);
        return Result<TeacherDto>.Success(TeacherMapping.ToDto(teacher));
    }
}

public sealed class DeleteTeacherCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteTeacherCommand, Result>
{
    public async Task<Result> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var teacher = await db.Teachers.FirstOrDefaultAsync(
            t => t.ExternalId == request.ExternalId && !t.IsDeleted, cancellationToken);
        if (teacher is null)
        {
            return Result.Failure(Error.NotFound("Teacher not found."));
        }

        teacher.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
