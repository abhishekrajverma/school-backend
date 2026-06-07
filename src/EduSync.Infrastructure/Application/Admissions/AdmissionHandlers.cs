using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Admissions.Application;
using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.Modules.Admissions.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Admissions;

public sealed class ListAdmissionsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListAdmissionsQuery, Result<PaginatedList<AdmissionListItemDto>>>
{
    // list admissions query handler is used to list all admissions based on the status passed in the request 
    public async Task<Result<PaginatedList<AdmissionListItemDto>>> Handle(
        ListAdmissionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.AdmissionApplications.AsNoTracking().Where(a => !a.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(a => a.Status == request.Status);
        }

        query = query.OrderByDescending(a => a.CreatedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, cancellationToken);
        var items = page.Items.Select(AdmissionJsonHelper.ToListItem).ToList();
        return Result<PaginatedList<AdmissionListItemDto>>.Success(
            PaginatedList<AdmissionListItemDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}
// get admission by id query handler is used to get an admission by its external id 
public sealed class GetAdmissionByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetAdmissionByIdQuery, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(GetAdmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var app = await db.AdmissionApplications.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ExternalId == request.ExternalId && !a.IsDeleted, cancellationToken);
        return app is null
            ? Result<AdmissionDetailDto>.Failure(Error.NotFound("Admission application not found."))
            : Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}

public sealed class CreateAdmissionCommandHandler(EduSyncDbContext db, ITenantContext tenantContext)
    : IRequestHandler<CreateAdmissionCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(CreateAdmissionCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            return Result<AdmissionDetailDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var formJson = AdmissionJsonHelper.SerializeForm(request.Request.FormData);
        var app = new AdmissionApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            ApplicationNo = $"ADM{DateTime.UtcNow:yyyy}{Random.Shared.Next(100000, 999999)}",
            Status = AdmissionStatuses.Draft,
            CurrentStep = request.Request.CurrentStep ?? "personal",
        };
        AdmissionJsonHelper.ApplyFormMetadata(app, formJson);

        db.AdmissionApplications.Add(app);
        await db.SaveChangesAsync(cancellationToken);
        return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}

public sealed class UpdateAdmissionCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateAdmissionCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(UpdateAdmissionCommand request, CancellationToken cancellationToken)
    {
        var app = await db.AdmissionApplications.FirstOrDefaultAsync(
            a => a.ExternalId == request.ExternalId && !a.IsDeleted, cancellationToken);
        if (app is null)
        {
            return Result<AdmissionDetailDto>.Failure(Error.NotFound("Admission application not found."));
        }

        if (app.Status != AdmissionStatuses.Draft)
        {
            return Result<AdmissionDetailDto>.Failure(Error.Conflict("Only draft applications can be edited."));
        }

        if (!string.IsNullOrWhiteSpace(request.Request.CurrentStep))
        {
            app.CurrentStep = request.Request.CurrentStep;
        }

        if (request.Request.FormData is not null)
        {
            AdmissionJsonHelper.ApplyFormMetadata(app, AdmissionJsonHelper.SerializeForm(request.Request.FormData));
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}

public sealed class UpdateAdmissionStatusCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateAdmissionStatusCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(
        UpdateAdmissionStatusCommand request,
        CancellationToken cancellationToken)
    {
        var status = request.Request.Status.Trim().ToLowerInvariant();
        if (!AdmissionStatuses.All.Contains(status))
        {
            return Result<AdmissionDetailDto>.Failure(Error.Validation("Invalid admission status."));
        }

        var app = await db.AdmissionApplications.FirstOrDefaultAsync(
            a => a.ExternalId == request.ExternalId && !a.IsDeleted, cancellationToken);
        if (app is null)
        {
            return Result<AdmissionDetailDto>.Failure(Error.NotFound("Admission application not found."));
        }

        app.Status = status;
        await db.SaveChangesAsync(cancellationToken);
        return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}

public sealed class SubmitAdmissionCommandHandler(EduSyncDbContext db)
    : IRequestHandler<SubmitAdmissionCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(SubmitAdmissionCommand request, CancellationToken cancellationToken)
    {
        var app = await db.AdmissionApplications.FirstOrDefaultAsync(
            a => a.ExternalId == request.ExternalId && !a.IsDeleted, cancellationToken);
        if (app is null)
        {
            return Result<AdmissionDetailDto>.Failure(Error.NotFound("Admission application not found."));
        }

        if (app.Status != AdmissionStatuses.Draft)
        {
            return Result<AdmissionDetailDto>.Failure(Error.Conflict("Application is already submitted."));
        }

        app.Status = AdmissionStatuses.Submitted;
        app.CurrentStep = "review";
        app.SubmittedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}

public sealed class RegisterAdmissionDocumentCommandHandler(EduSyncDbContext db)
    : IRequestHandler<RegisterAdmissionDocumentCommand, Result<AdmissionDocumentDto>>
{
    public async Task<Result<AdmissionDocumentDto>> Handle(
        RegisterAdmissionDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var app = await db.AdmissionApplications.FirstOrDefaultAsync(
            a => a.ExternalId == request.ExternalId && !a.IsDeleted, cancellationToken);
        if (app is null)
        {
            return Result<AdmissionDocumentDto>.Failure(Error.NotFound("Admission application not found."));
        }

        var doc = new AdmissionDocumentDto(
            request.Request.DocumentType,
            request.Request.FileName,
            request.Request.ContentType,
            request.Request.Size,
            request.Request.StorageUrl ?? $"pending://admissions/{app.ExternalId}/{request.Request.DocumentType}",
            DateTime.UtcNow);

        app.DocumentsJson = AdmissionJsonHelper.AppendDocument(app.DocumentsJson, doc);
        await db.SaveChangesAsync(cancellationToken);
        return Result<AdmissionDocumentDto>.Success(doc);
    }
}
