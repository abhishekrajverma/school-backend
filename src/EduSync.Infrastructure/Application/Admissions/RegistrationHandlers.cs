using EduSync.Infrastructure.Events;
using EduSync.Infrastructure.MultiRegion;
using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Admissions.Application;
using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.Modules.Admissions.Domain;
using EduSync.Modules.Events.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Admissions;

internal static class RegistrationMapping
{
    public static RegistrationListItemDto ToListItem(Registration r) => new(
        r.ExternalId,
        r.RegistrationNo,
        r.Source,
        r.Status,
        $"{r.ApplicantFirstName} {r.ApplicantLastName}".Trim(),
        r.ClassSought,
        r.CreatedAt,
        r.SubmittedAt);

    public static RegistrationDetailDto ToDetail(Registration r) => new(
        r.ExternalId,
        r.RegistrationNo,
        r.Source,
        r.Status,
        r.ApplicantFirstName,
        r.ApplicantLastName,
        r.ApplicantEmail,
        r.ApplicantPhone,
        r.ClassSought,
        r.AcademicYearId.ToString(),
        AdmissionJsonHelper.ParseForm(r.FormDataJson),
        r.CreatedAt,
        r.SubmittedAt);
}

public sealed class ListRegistrationsQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListRegistrationsQuery, Result<PaginatedList<RegistrationListItemDto>>>
{
    public async Task<Result<PaginatedList<RegistrationListItemDto>>> Handle(
        ListRegistrationsQuery request,
        CancellationToken ct)
    {
        var query = db.Registrations.AsNoTracking().Where(r => !r.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(r => r.Status == request.Status);
        }

        query = query.OrderByDescending(r => r.CreatedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(RegistrationMapping.ToListItem).ToList();
        return Result<PaginatedList<RegistrationListItemDto>>.Success(
            PaginatedList<RegistrationListItemDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetRegistrationByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetRegistrationByIdQuery, Result<RegistrationDetailDto>>
{
    public async Task<Result<RegistrationDetailDto>> Handle(GetRegistrationByIdQuery request, CancellationToken ct)
    {
        var reg = await db.Registrations.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId && !r.IsDeleted, ct);
        return reg is null
            ? Result<RegistrationDetailDto>.Failure(Error.NotFound("Registration not found."))
            : Result<RegistrationDetailDto>.Success(RegistrationMapping.ToDetail(reg));
    }
}

public sealed class CreateRegistrationCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch,
    IAcademicYearContext academicYear)
    : IRequestHandler<CreateRegistrationCommand, Result<RegistrationDetailDto>>
{
    public async Task<Result<RegistrationDetailDto>> Handle(CreateRegistrationCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue || !branch.BranchId.HasValue || !academicYear.AcademicYearId.HasValue)
        {
            return Result<RegistrationDetailDto>.Failure(Error.Forbidden("Tenant, branch, and academic year are required."));
        }

        var body = request.Request;
        var source = body.Source.Trim().ToLowerInvariant();
        if (!RegistrationSources.All.Contains(source))
        {
            return Result<RegistrationDetailDto>.Failure(Error.Validation("Invalid registration source."));
        }

        if (!string.IsNullOrWhiteSpace(body.ApplicantEmail))
        {
            var email = body.ApplicantEmail.Trim().ToLowerInvariant();
            var duplicate = await db.Registrations.AnyAsync(
                r => r.TenantId == tenant.TenantId
                     && r.AcademicYearId == academicYear.AcademicYearId
                     && r.ApplicantEmail != null
                     && r.ApplicantEmail.ToLower() == email
                     && r.Status != RegistrationStatuses.Cancelled
                     && !r.IsDeleted,
                ct);
            if (duplicate)
            {
                return Result<RegistrationDetailDto>.Failure(Error.Conflict("A registration with this email already exists for this academic year."));
            }
        }

        var reg = new Registration
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            BranchId = branch.BranchId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            RegistrationNo = $"REG{DateTime.UtcNow:yyyy}{Random.Shared.Next(100000, 999999)}",
            Source = source,
            Status = RegistrationStatuses.Draft,
            AcademicYearId = academicYear.AcademicYearId.Value,
            ApplicantFirstName = body.ApplicantFirstName.Trim(),
            ApplicantLastName = body.ApplicantLastName.Trim(),
            ApplicantEmail = body.ApplicantEmail?.Trim(),
            ApplicantPhone = body.ApplicantPhone?.Trim(),
            ClassSought = body.ClassSought?.Trim(),
            FormDataJson = body.FormData is not null ? AdmissionJsonHelper.SerializeForm(body.FormData) : "{}",
        };

        db.Registrations.Add(reg);
        await db.SaveChangesAsync(ct);
        return Result<RegistrationDetailDto>.Success(RegistrationMapping.ToDetail(reg));
    }
}

public sealed class UpdateRegistrationCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateRegistrationCommand, Result<RegistrationDetailDto>>
{
    public async Task<Result<RegistrationDetailDto>> Handle(UpdateRegistrationCommand request, CancellationToken ct)
    {
        var reg = await db.Registrations.FirstOrDefaultAsync(
            r => r.ExternalId == request.ExternalId && !r.IsDeleted, ct);
        if (reg is null)
        {
            return Result<RegistrationDetailDto>.Failure(Error.NotFound("Registration not found."));
        }

        if (reg.Status != RegistrationStatuses.Draft)
        {
            return Result<RegistrationDetailDto>.Failure(Error.Conflict("Only draft registrations can be edited."));
        }

        var body = request.Request;
        if (body.ApplicantFirstName is not null) reg.ApplicantFirstName = body.ApplicantFirstName.Trim();
        if (body.ApplicantLastName is not null) reg.ApplicantLastName = body.ApplicantLastName.Trim();
        if (body.ApplicantEmail is not null) reg.ApplicantEmail = body.ApplicantEmail.Trim();
        if (body.ApplicantPhone is not null) reg.ApplicantPhone = body.ApplicantPhone.Trim();
        if (body.ClassSought is not null) reg.ClassSought = body.ClassSought.Trim();
        if (body.FormData is not null) reg.FormDataJson = AdmissionJsonHelper.SerializeForm(body.FormData);

        await db.SaveChangesAsync(ct);
        return Result<RegistrationDetailDto>.Success(RegistrationMapping.ToDetail(reg));
    }
}

public sealed class SubmitRegistrationCommandHandler(
    EduSyncDbContext db,
    IIntegrationEventCollector events,
    ITenantContext tenant,
    IRegionContext region,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<SubmitRegistrationCommand, Result<RegistrationDetailDto>>
{
    public async Task<Result<RegistrationDetailDto>> Handle(SubmitRegistrationCommand request, CancellationToken ct)
    {
        var reg = await db.Registrations.FirstOrDefaultAsync(
            r => r.ExternalId == request.ExternalId && !r.IsDeleted, ct);
        if (reg is null)
        {
            return Result<RegistrationDetailDto>.Failure(Error.NotFound("Registration not found."));
        }

        var submitResult = reg.Submit();
        if (!submitResult.IsSuccess)
        {
            return Result<RegistrationDetailDto>.Failure(submitResult.Error!);
        }

        events.Add(IntegrationEventFactory.Create(
            IntegrationEventTypes.RegistrationSubmitted,
            new { reg.ExternalId, reg.RegistrationNo },
            tenant,
            region,
            httpContextAccessor));
        await db.SaveChangesAsync(ct);
        return Result<RegistrationDetailDto>.Success(RegistrationMapping.ToDetail(reg));
    }
}

public sealed class ConvertRegistrationToAdmissionCommandHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IBranchContext branch,
    IAcademicYearContext academicYear)
    : IRequestHandler<ConvertRegistrationToAdmissionCommand, Result<AdmissionDetailDto>>
{
    public async Task<Result<AdmissionDetailDto>> Handle(
        ConvertRegistrationToAdmissionCommand request,
        CancellationToken ct)
    {
        var reg = await db.Registrations.FirstOrDefaultAsync(
            r => r.ExternalId == request.ExternalId && !r.IsDeleted, ct);
        if (reg is null)
        {
            return Result<AdmissionDetailDto>.Failure(Error.NotFound("Registration not found."));
        }

        if (reg.Status is RegistrationStatuses.Cancelled or RegistrationStatuses.Converted)
        {
            return Result<AdmissionDetailDto>.Failure(Error.Conflict("Registration cannot be converted."));
        }

        var existing = await db.AdmissionApplications
            .FirstOrDefaultAsync(a => a.RegistrationId == reg.Id && !a.IsDeleted, ct);
        if (existing is not null)
        {
            return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(existing));
        }

        var source = request.Request?.Source?.Trim().ToLowerInvariant() ?? reg.Source;
        if (!AdmissionSources.All.Contains(source))
        {
            source = AdmissionSources.Online;
        }

        var app = new AdmissionApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId!.Value,
            BranchId = reg.BranchId,
            ExternalId = Guid.NewGuid().ToString("N")[..12],
            ApplicationNo = $"ADM{DateTime.UtcNow:yyyy}{Random.Shared.Next(100000, 999999)}",
            RegistrationId = reg.Id,
            AcademicYearId = reg.AcademicYearId,
            Source = source,
            Status = AdmissionStatuses.Draft,
            CurrentStep = "personal",
            FormDataJson = reg.FormDataJson,
            ApplicantName = $"{reg.ApplicantFirstName} {reg.ApplicantLastName}".Trim(),
            ClassSought = reg.ClassSought,
        };

        db.AdmissionApplications.Add(app);
        var convertResult = reg.MarkConverted();
        if (!convertResult.IsSuccess)
        {
            return Result<AdmissionDetailDto>.Failure(convertResult.Error!);
        }

        await db.SaveChangesAsync(ct);
        return Result<AdmissionDetailDto>.Success(AdmissionJsonHelper.ToDetail(app));
    }
}
