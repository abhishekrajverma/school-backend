using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Tenancy.Application.Dtos;

public sealed record FinancialYearDto(
    string Id,
    string Name,
    string StartDate,
    string EndDate,
    bool IsCurrent);

public sealed record FinancialYearSettingsDto(
    IReadOnlyList<FinancialYearDto> Years,
    string? DefaultYear,
    bool HideInUi);

public sealed record SetCurrentFinancialYearRequest(string Name);

public sealed record GetFinancialYearSettingsQuery : IRequest<Result<FinancialYearSettingsDto>>;
public sealed record SetCurrentFinancialYearCommand(SetCurrentFinancialYearRequest Request)
    : IRequest<Result<FinancialYearSettingsDto>>;
