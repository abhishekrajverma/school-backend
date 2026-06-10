using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Admissions.Application;
using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.Modules.Admissions.Domain;
using EduSync.Modules.Events.Domain;
using EduSync.Modules.Students.Domain;
using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Admissions;

public sealed class ListAdmissionsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListAdmissionsQuery, Result<PaginatedList<AdmissionListItemDto>>>
{
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

public sealed class CreateAdmissionCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenantContext,
    IBranchContext branchContext,
    IAcademicYearContext academicYearContext)
    : IRequestHandler<CreateAdmissionCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(CreateAdmissionCommand request, CancellationToken cancellationToken)
    {
        if (!tenantContext.TenantId.HasValue || !branchContext.BranchId.HasValue || !academicYearContext.AcademicYearId.HasValue)
        {
            return Result<AdmissionDetailDto>.Failure(Error.Forbidden("Tenant, branch, and academic year are required."));
        }

        var formJson = AdmissionJsonHelper.SerializeForm(request.Request.FormData);
        var app = new AdmissionApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantContext.TenantId.Value,
            BranchId = branchContext.BranchId.Value,
            AcademicYearId = academicYearContext.AcademicYearId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            ApplicationNo = $"ADM{DateTime.UtcNow:yyyy}{Random.Shared.Next(100000, 999999)}",
            Source = AdmissionSources.Online,
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

        var transition = app.TransitionTo(status);
        if (!transition.IsSuccess)
        {
            return Result<AdmissionDetailDto>.Failure(transition.Error!);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}

public sealed class ApproveAdmissionCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch,
    ICurrentUserContext user,
    IIntegrationEventCollector events,
    IRegionContext region,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<ApproveAdmissionCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(ApproveAdmissionCommand request, CancellationToken ct)
    {
        var app = await db.AdmissionApplications.FirstOrDefaultAsync(
            a => a.ExternalId == request.ExternalId && !a.IsDeleted, ct);
        if (app is null)
        {
            return Result<AdmissionDetailDto>.Failure(Error.NotFound("Admission application not found."));
        }

        if (app.Status == AdmissionStatuses.Approved && !string.IsNullOrEmpty(app.ApprovedStudentExternalId))
        {
            return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
        }

        if (user.UserId is null)
        {
            return Result<AdmissionDetailDto>.Failure(Error.Forbidden("Authenticated user required."));
        }

        var approveResult = app.Approve(user.UserId.Value, request.Request?.Remarks);
        if (!approveResult.IsSuccess)
        {
            return Result<AdmissionDetailDto>.Failure(approveResult.Error!);
        }

        var fields = AdmissionJsonHelper.ParseStudentFields(app.FormDataJson);
        var admissionNo = app.ApplicationNo;

        if (await db.Students.AnyAsync(
                s => s.TenantId == app.TenantId && s.AdmissionNo == admissionNo && !s.IsDeleted, ct))
        {
            admissionNo = $"{app.ApplicationNo}-{Random.Shared.Next(100, 999)}";
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            TenantId = app.TenantId,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            FirstName = fields.FirstName,
            LastName = fields.LastName,
            Email = fields.Email,
            Phone = fields.Phone,
            AdmissionNo = admissionNo,
            LifecycleStatus = LifecycleStatuses.Active,
            AdmissionApplicationId = app.Id,
        };

        var enrollment = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            TenantId = app.TenantId,
            BranchId = app.BranchId,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            StudentId = student.Id,
            AcademicYearId = app.AcademicYearId,
            ClassName = app.ClassSought ?? "Unassigned",
            Section = fields.Section ?? "A",
            RollNo = fields.RollNo ?? "0",
            EnrollmentStatus = EnrollmentStatuses.Enrolled,
            EnrolledAt = DateTime.UtcNow,
        };

        db.Students.Add(student);
        db.StudentEnrollments.Add(enrollment);

        app.ApprovedStudentExternalId = student.ExternalId;

        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.AdmissionApproved,
            new { admissionExternalId = app.ExternalId, studentExternalId = student.ExternalId, enrollmentExternalId = enrollment.ExternalId },
            tenant,
            region,
            httpContextAccessor));
        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.StudentCreated,
            new { studentExternalId = student.ExternalId, admissionNo = student.AdmissionNo, className = enrollment.ClassName },
            tenant,
            region,
            httpContextAccessor));
        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.StudentEnrolled,
            new { studentExternalId = student.ExternalId, enrollmentExternalId = enrollment.ExternalId, className = enrollment.ClassName },
            tenant,
            region,
            httpContextAccessor));

        await db.SaveChangesAsync(ct);
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

        var submitResult = app.Submit();
        if (!submitResult.IsSuccess)
        {
            return Result<AdmissionDetailDto>.Failure(submitResult.Error!);
        }

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
