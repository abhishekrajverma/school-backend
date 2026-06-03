using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Admissions.Application;

public sealed record ListAdmissionsQuery(PaginationQuery Pagination, string? Status)
    : IRequest<Result<PaginatedList<AdmissionListItemDto>>>;

public sealed record GetAdmissionByIdQuery(string ExternalId) : IRequest<Result<AdmissionDetailDto>>;
public sealed record CreateAdmissionCommand(CreateAdmissionRequest Request) : IRequest<Result<AdmissionDetailDto>>;
public sealed record UpdateAdmissionCommand(string ExternalId, UpdateAdmissionRequest Request) : IRequest<Result<AdmissionDetailDto>>;
public sealed record UpdateAdmissionStatusCommand(string ExternalId, UpdateAdmissionStatusRequest Request) : IRequest<Result<AdmissionDetailDto>>;
public sealed record RegisterAdmissionDocumentCommand(string ExternalId, RegisterAdmissionDocumentRequest Request)
    : IRequest<Result<AdmissionDocumentDto>>;

public sealed record SubmitAdmissionCommand(string ExternalId) : IRequest<Result<AdmissionDetailDto>>;
