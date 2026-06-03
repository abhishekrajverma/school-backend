using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Tenancy.Application.Queries;

public sealed record GetCurrentTenantQuery(Guid TenantId) : IRequest<Result<CurrentTenantDto>>;
