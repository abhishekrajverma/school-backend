using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Parents.Application;
using EduSync.Modules.Parents.Application.Dtos;
using EduSync.Modules.Parents.Domain;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Parents;

internal static class ParentLinkHelper
{
    public static async Task<(IReadOnlyList<string> Children, IReadOnlyList<string> StudentIds)> LoadChildInfoAsync(
        EduSyncDbContext db,
        Guid parentId,
        CancellationToken ct)
    {
        var links = await db.StudentParents.AsNoTracking()
            .Where(sp => sp.ParentId == parentId && sp.IsActive && !sp.IsDeleted)
            .Join(db.Students.AsNoTracking(),
                sp => sp.StudentId,
                s => s.Id,
                (sp, s) => new { s.ExternalId, s.FullName })
            .ToListAsync(ct);

        return (
            links.Select(l => l.FullName).ToList(),
            links.Select(l => l.ExternalId).ToList());
    }

    public static async Task SyncStudentLinksAsync(
        EduSyncDbContext db,
        Guid tenantId,
        Parent parent,
        IReadOnlyList<string>? studentExternalIds,
        CancellationToken ct)
    {
        if (studentExternalIds is null)
        {
            return;
        }

        var students = await db.Students
            .Where(s => studentExternalIds.Contains(s.ExternalId) && !s.IsDeleted)
            .ToListAsync(ct);

        var existing = await db.StudentParents
            .Where(sp => sp.ParentId == parent.Id && !sp.IsDeleted)
            .ToListAsync(ct);

        foreach (var link in existing)
        {
            link.IsActive = false;
            link.ValidTo = DateOnly.FromDateTime(DateTime.UtcNow);
        }

        foreach (var student in students)
        {
            db.StudentParents.Add(new StudentParent
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ExternalId = Guid.NewGuid().ToString("N")[..12],
                ParentId = parent.Id,
                StudentId = student.Id,
                Relationship = "guardian",
                IsPrimary = true,
                IsActive = true,
                ValidFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            });
        }
    }
}

public sealed class ListParentsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListParentsQuery, Result<PaginatedList<ParentDto>>>
{
    public async Task<Result<PaginatedList<ParentDto>>> Handle(ListParentsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Parents.AsNoTracking().Where(p => !p.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(p =>
                p.FirstName.ToLower().Contains(term) ||
                p.LastName.ToLower().Contains(term) ||
                p.Email.ToLower().Contains(term));
        }

        query = request.Pagination.IsDescending
            ? query.OrderByDescending(p => p.LastName)
            : query.OrderBy(p => p.LastName);

        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, cancellationToken);
        var items = new List<ParentDto>();
        foreach (var parent in page.Items)
        {
            var (children, studentIds) = await ParentLinkHelper.LoadChildInfoAsync(db, parent.Id, cancellationToken);
            items.Add(ParentMapping.ToDto(parent, children, studentIds));
        }

        return Result<PaginatedList<ParentDto>>.Success(
            PaginatedList<ParentDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetParentByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetParentByIdQuery, Result<ParentDto>>
{
    public async Task<Result<ParentDto>> Handle(GetParentByIdQuery request, CancellationToken cancellationToken)
    {
        var parent = await db.Parents.AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId && !p.IsDeleted, cancellationToken);
        if (parent is null)
        {
            return Result<ParentDto>.Failure(Error.NotFound("Parent not found."));
        }

        var (children, studentIds) = await ParentLinkHelper.LoadChildInfoAsync(db, parent.Id, cancellationToken);
        return Result<ParentDto>.Success(ParentMapping.ToDto(parent, children, studentIds));
    }
}

public sealed class CreateParentCommandHandler(EduSyncDbContext db, ITenantContext tenantContext)
    : IRequestHandler<CreateParentCommand, Result<ParentDto>>
{
    public async Task<Result<ParentDto>> Handle(CreateParentCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return Result<ParentDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var body = request.Request;
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FirstName = body.FirstName.Trim(),
            LastName = body.LastName.Trim(),
            Email = body.Email.Trim(),
            Phone = body.Phone,
            Occupation = body.Occupation,
            Address = body.Address,
            LifecycleStatus = body.Status,
        };

        db.Parents.Add(parent);
        await ParentLinkHelper.SyncStudentLinksAsync(
            db, tenantContext.TenantId.Value, parent, body.StudentIds, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var (children, studentIds) = await ParentLinkHelper.LoadChildInfoAsync(db, parent.Id, cancellationToken);
        return Result<ParentDto>.Success(ParentMapping.ToDto(parent, children, studentIds));
    }
}

public sealed class UpdateParentCommandHandler(EduSyncDbContext db, ITenantContext tenantContext)
    : IRequestHandler<UpdateParentCommand, Result<ParentDto>>
{
    public async Task<Result<ParentDto>> Handle(UpdateParentCommand request, CancellationToken cancellationToken)
    {
        var parent = await db.Parents.FirstOrDefaultAsync(
            p => p.ExternalId == request.ExternalId && !p.IsDeleted, cancellationToken);
        if (parent is null)
        {
            return Result<ParentDto>.Failure(Error.NotFound("Parent not found."));
        }

        var body = request.Request;
        if (body.FirstName is not null) parent.FirstName = body.FirstName.Trim();
        if (body.LastName is not null) parent.LastName = body.LastName.Trim();
        if (body.Email is not null) parent.Email = body.Email.Trim();
        if (body.Phone is not null) parent.Phone = body.Phone;
        if (body.Occupation is not null) parent.Occupation = body.Occupation;
        if (body.Address is not null) parent.Address = body.Address;
        if (body.Status is not null) parent.LifecycleStatus = body.Status;

        if (body.StudentIds is not null && tenantContext.TenantId.HasValue)
        {
            await ParentLinkHelper.SyncStudentLinksAsync(
                db, tenantContext.TenantId.Value, parent, body.StudentIds, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        var (children, studentIds) = await ParentLinkHelper.LoadChildInfoAsync(db, parent.Id, cancellationToken);
        return Result<ParentDto>.Success(ParentMapping.ToDto(parent, children, studentIds));
    }
}

public sealed class DeleteParentCommandHandler(EduSyncDbContext db)
    : IRequestHandler<DeleteParentCommand, Result>
{
    public async Task<Result> Handle(DeleteParentCommand request, CancellationToken cancellationToken)
    {
        var parent = await db.Parents.FirstOrDefaultAsync(
            p => p.ExternalId == request.ExternalId && !p.IsDeleted, cancellationToken);
        if (parent is null)
        {
            return Result.Failure(Error.NotFound("Parent not found."));
        }

        parent.IsDeleted = true;
        parent.LifecycleStatus = "inactive";
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
