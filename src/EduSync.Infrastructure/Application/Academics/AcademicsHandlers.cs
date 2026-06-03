using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Academics.Application;
using EduSync.Modules.Academics.Application.Dtos;
using EduSync.Modules.Academics.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Academics;

public sealed class ListClassesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListClassesQuery, Result<IReadOnlyList<ClassDto>>>
{
    public async Task<Result<IReadOnlyList<ClassDto>>> Handle(ListClassesQuery request, CancellationToken cancellationToken)
    {
        var items = await db.Classes.AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<ClassDto>>.Success(items.Select(AcademicsMapping.ToDto).ToList());
    }
}

public sealed class ListSubjectsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListSubjectsQuery, Result<IReadOnlyList<SubjectDto>>>
{
    public async Task<Result<IReadOnlyList<SubjectDto>>> Handle(ListSubjectsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Subjects.AsNoTracking().Where(s => !s.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.ClassName))
        {
            query = query.Where(s => s.ClassName == request.ClassName);
        }

        var items = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
        return Result<IReadOnlyList<SubjectDto>>.Success(items.Select(AcademicsMapping.ToDto).ToList());
    }
}

public sealed class CreateClassCommandHandler(EduSyncDbContext db, ITenantContext tenantContext)
    : IRequestHandler<CreateClassCommand, Result<ClassDto>>
{
    public async Task<Result<ClassDto>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return Result<ClassDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var entity = new SchoolClass
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            Name = request.Request.Name.Trim(),
            SectionsJson = AcademicsMapping.SerializeSections(request.Request.Sections),
            TotalStudents = request.Request.TotalStudents,
            ClassTeacherName = request.Request.ClassTeacher,
        };

        db.Classes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Result<ClassDto>.Success(AcademicsMapping.ToDto(entity));
    }
}

public sealed class CreateSubjectCommandHandler(EduSyncDbContext db, ITenantContext tenantContext)
    : IRequestHandler<CreateSubjectCommand, Result<SubjectDto>>
{
    public async Task<Result<SubjectDto>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return Result<SubjectDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var body = request.Request;
        var entity = new Subject
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            Name = body.Name.Trim(),
            Code = body.Code.Trim(),
            ClassName = body.Class.Trim(),
            TeacherExternalId = body.TeacherId,
            TeacherName = body.TeacherName,
            WeeklyHours = body.WeeklyHours,
            Status = body.Status,
        };

        db.Subjects.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Result<SubjectDto>.Success(AcademicsMapping.ToDto(entity));
    }
}
