using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Parents.Application;
using EduSync.Modules.Parents.Application.Dtos;
using EduSync.Modules.Parents.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Parents;

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
        var items = page.Items.Select(ParentMapping.ToDto).ToList();
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
        return parent is null
            ? Result<ParentDto>.Failure(Error.NotFound("Parent not found."))
            : Result<ParentDto>.Success(ParentMapping.ToDto(parent));
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
            Status = body.Status,
            ChildrenJson = ParentMapping.SerializeList(body.Children),
            StudentIdsJson = ParentMapping.SerializeList(body.StudentIds),
        };

        db.Parents.Add(parent);
        await db.SaveChangesAsync(cancellationToken);
        return Result<ParentDto>.Success(ParentMapping.ToDto(parent));
    }
}

public sealed class UpdateParentCommandHandler(EduSyncDbContext db)
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
        if (body.Status is not null) parent.Status = body.Status;
        if (body.Children is not null) parent.ChildrenJson = ParentMapping.SerializeList(body.Children);
        if (body.StudentIds is not null) parent.StudentIdsJson = ParentMapping.SerializeList(body.StudentIds);

        await db.SaveChangesAsync(cancellationToken);
        return Result<ParentDto>.Success(ParentMapping.ToDto(parent));
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
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
