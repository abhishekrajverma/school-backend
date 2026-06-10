using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Admissions.Application;

public sealed record ListRegistrationsQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<RegistrationListItemDto>>>;

public sealed record GetRegistrationByIdQuery(string ExternalId) : IRequest<Result<RegistrationDetailDto>>;
public sealed record CreateRegistrationCommand(CreateRegistrationRequest Request) : IRequest<Result<RegistrationDetailDto>>;
public sealed record UpdateRegistrationCommand(string ExternalId, UpdateRegistrationRequest Request) : IRequest<Result<RegistrationDetailDto>>;
public sealed record SubmitRegistrationCommand(string ExternalId) : IRequest<Result<RegistrationDetailDto>>;
public sealed record ConvertRegistrationToAdmissionCommand(string ExternalId, ConvertRegistrationToAdmissionRequest? Request)
    : IRequest<Result<AdmissionDetailDto>>;
