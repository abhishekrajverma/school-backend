using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Company.Application;

public sealed record EnquiryDto(
    string Id,
    string SchoolName,
    string ContactName,
    string Email,
    string? Phone,
    string? City,
    string? PlanKey,
    string Status,
    string? Notes,
    string? TenantExternalId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateEnquiryRequest(
    string SchoolName,
    string ContactName,
    string Email,
    string? Phone,
    string? City,
    string? PlanKey);

public sealed record UpdateEnquiryRequest(
    string? Status,
    string? Notes,
    string? PlanKey);

public sealed record CompanyOverviewDto(
    int TotalSchools,
    int LiveSchools,
    int PendingSchools,
    int SuspendedSchools,
    int NewEnquiries,
    IReadOnlyList<ManagedSchoolDto> Schools,
    IReadOnlyList<EnquiryDto> RecentEnquiries);

public sealed record ManagedSchoolDto(
    string Id,
    string Slug,
    string Name,
    string? SchoolEmail,
    string Status,
    string PlanKey,
    DateTime CreatedAt);

public sealed record CompanyActionRequest(
    string Action,
    string? TenantId,
    string? PlanKey,
    string? Status);

public sealed record CreateEnquiryCommand(CreateEnquiryRequest Request) : IRequest<Result<EnquiryDto>>;
public sealed record ListEnquiriesQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<EnquiryDto>>>;
public sealed record GetEnquiryByIdQuery(string ExternalId) : IRequest<Result<EnquiryDto>>;
public sealed record UpdateEnquiryCommand(string ExternalId, UpdateEnquiryRequest Request) : IRequest<Result<EnquiryDto>>;
public sealed record GetCompanyOverviewQuery : IRequest<Result<CompanyOverviewDto>>;
public sealed record ExecuteCompanyActionCommand(CompanyActionRequest Request) : IRequest<Result<object>>;
