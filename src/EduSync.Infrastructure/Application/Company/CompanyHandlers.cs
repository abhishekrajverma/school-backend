using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Modules.Company.Application;
using EduSync.Modules.Company.Domain;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Company;

internal static class EnquiryMapping
{
    public static EnquiryDto ToDto(SchoolEnquiry e) => new(
        e.ExternalId,
        e.SchoolName,
        e.ContactName,
        e.Email,
        e.Phone,
        e.City,
        e.PlanKey,
        e.Status,
        e.Notes,
        e.TenantExternalId,
        e.CreatedAt,
        e.UpdatedAt);
}

public sealed class CreateEnquiryCommandHandler(EduSyncDbContext db)
    : IRequestHandler<CreateEnquiryCommand, Result<EnquiryDto>>
{
    public async Task<Result<EnquiryDto>> Handle(CreateEnquiryCommand request, CancellationToken ct)
    {
        var body = request.Request;
        if (string.IsNullOrWhiteSpace(body.SchoolName) || string.IsNullOrWhiteSpace(body.ContactName) || string.IsNullOrWhiteSpace(body.Email))
        {
            return Result<EnquiryDto>.Failure(Error.Validation("School name, contact name, and email are required."));
        }

        var count = await db.SchoolEnquiries.CountAsync(ct);
        var enquiry = new SchoolEnquiry
        {
            Id = Guid.NewGuid(),
            ExternalId = (count + 1).ToString(),
            SchoolName = body.SchoolName.Trim(),
            ContactName = body.ContactName.Trim(),
            Email = body.Email.Trim().ToLowerInvariant(),
            Phone = body.Phone?.Trim(),
            City = body.City?.Trim(),
            PlanKey = body.PlanKey?.Trim().ToLowerInvariant(),
            Status = EnquiryStatuses.New,
            CreatedAt = DateTime.UtcNow,
        };

        db.SchoolEnquiries.Add(enquiry);
        await db.SaveChangesAsync(ct);
        return Result<EnquiryDto>.Success(EnquiryMapping.ToDto(enquiry));
    }
}

public sealed class ListEnquiriesQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListEnquiriesQuery, Result<PaginatedList<EnquiryDto>>>
{
    public async Task<Result<PaginatedList<EnquiryDto>>> Handle(ListEnquiriesQuery request, CancellationToken ct)
    {
        var query = db.SchoolEnquiries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Pagination.Search))
        {
            var term = request.Pagination.Search.ToLowerInvariant();
            query = query.Where(e =>
                e.SchoolName.ToLower().Contains(term) ||
                e.ContactName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term));
        }

        query = query.OrderByDescending(e => e.CreatedAt);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(EnquiryMapping.ToDto).ToList();
        return Result<PaginatedList<EnquiryDto>>.Success(
            PaginatedList<EnquiryDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetEnquiryByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetEnquiryByIdQuery, Result<EnquiryDto>>
{
    public async Task<Result<EnquiryDto>> Handle(GetEnquiryByIdQuery request, CancellationToken ct)
    {
        var enquiry = await db.SchoolEnquiries.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExternalId == request.ExternalId, ct);
        return enquiry is null
            ? Result<EnquiryDto>.Failure(Error.NotFound("Enquiry not found."))
            : Result<EnquiryDto>.Success(EnquiryMapping.ToDto(enquiry));
    }
}

public sealed class UpdateEnquiryCommandHandler(EduSyncDbContext db)
    : IRequestHandler<UpdateEnquiryCommand, Result<EnquiryDto>>
{
    public async Task<Result<EnquiryDto>> Handle(UpdateEnquiryCommand request, CancellationToken ct)
    {
        var enquiry = await db.SchoolEnquiries.FirstOrDefaultAsync(e => e.ExternalId == request.ExternalId, ct);
        if (enquiry is null)
        {
            return Result<EnquiryDto>.Failure(Error.NotFound("Enquiry not found."));
        }

        var body = request.Request;
        if (body.Status is not null)
        {
            var status = body.Status.Trim().ToLowerInvariant();
            if (status is not (EnquiryStatuses.New or EnquiryStatuses.Contacted or EnquiryStatuses.Converted or EnquiryStatuses.Rejected))
            {
                return Result<EnquiryDto>.Failure(Error.Validation("Invalid enquiry status."));
            }

            enquiry.Status = status;
        }

        if (body.Notes is not null) enquiry.Notes = body.Notes;
        if (body.PlanKey is not null) enquiry.PlanKey = body.PlanKey.Trim().ToLowerInvariant();
        enquiry.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result<EnquiryDto>.Success(EnquiryMapping.ToDto(enquiry));
    }
}

public sealed class GetCompanyOverviewQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetCompanyOverviewQuery, Result<CompanyOverviewDto>>
{
    public async Task<Result<CompanyOverviewDto>> Handle(GetCompanyOverviewQuery request, CancellationToken ct)
    {
        var tenants = await db.Tenants.AsNoTracking()
            .Include(t => t.Subscription)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var schools = tenants.Select(t => new ManagedSchoolDto(
            t.ExternalId,
            t.Slug,
            t.Name,
            t.SchoolEmail,
            TenantStatusMapper.ToApiStatus(t.Status),
            t.Subscription?.PlanId ?? "starter",
            t.CreatedAt)).ToList();

        var recent = await db.SchoolEnquiries.AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .Take(10)
            .ToListAsync(ct);

        var newEnquiries = await db.SchoolEnquiries.CountAsync(e => e.Status == EnquiryStatuses.New, ct);

        return Result<CompanyOverviewDto>.Success(new CompanyOverviewDto(
            tenants.Count,
            tenants.Count(t => t.Status == TenantStatus.Active),
            tenants.Count(t => t.Status == TenantStatus.Provisioning),
            tenants.Count(t => t.Status == TenantStatus.Suspended),
            newEnquiries,
            schools,
            recent.Select(EnquiryMapping.ToDto).ToList()));
    }
}

public sealed class ExecuteCompanyActionCommandHandler(EduSyncDbContext db)
    : IRequestHandler<ExecuteCompanyActionCommand, Result<object>>
{
    public async Task<Result<object>> Handle(ExecuteCompanyActionCommand request, CancellationToken ct)
    {
        var body = request.Request;
        if (string.IsNullOrWhiteSpace(body.Action))
        {
            return Result<object>.Failure(Error.Validation("Action is required."));
        }

        var action = body.Action.Trim().ToLowerInvariant();
        if (action is "activate" or "suspend" or "assign-plan")
        {
            if (string.IsNullOrWhiteSpace(body.TenantId))
            {
                return Result<object>.Failure(Error.Validation("TenantId is required."));
            }

            var tenant = await db.Tenants
                .Include(t => t.Subscription)
                .FirstOrDefaultAsync(t => t.ExternalId == body.TenantId || t.Slug == body.TenantId, ct);

            if (tenant is null)
            {
                return Result<object>.Failure(Error.NotFound("Tenant not found."));
            }

            switch (action)
            {
                case "activate":
                    tenant.Status = TenantStatus.Active;
                    break;
                case "suspend":
                    tenant.Status = TenantStatus.Suspended;
                    break;
                case "assign-plan":
                    if (string.IsNullOrWhiteSpace(body.PlanKey))
                    {
                        return Result<object>.Failure(Error.Validation("PlanKey is required for assign-plan."));
                    }

                    tenant.Subscription ??= new TenantSubscription { TenantId = tenant.Id };
                    tenant.Subscription.PlanId = body.PlanKey.Trim().ToLowerInvariant();
                    tenant.Subscription.SeatLimit = ResolveSeatLimit(tenant.Subscription.PlanId);
                    break;
            }

            await db.SaveChangesAsync(ct);
            return Result<object>.Success(new
            {
                tenantId = tenant.ExternalId,
                status = TenantStatusMapper.ToApiStatus(tenant.Status),
                planKey = tenant.Subscription?.PlanId,
            });
        }

        return Result<object>.Failure(Error.Validation($"Unknown action: {body.Action}"));
    }

    private static int ResolveSeatLimit(string planId) => planId.ToLowerInvariant() switch
    {
        "enterprise" => 500,
        "professional" => 150,
        _ => 50,
    };
}
