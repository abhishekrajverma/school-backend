using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduSync.Infrastructure.Application.Tenancy;

public sealed class GetFinancialYearSettingsQueryHandler(
    EduSyncDbContext db,
    ITenantContext tenant,
    IConfiguration configuration)
    : IRequestHandler<GetFinancialYearSettingsQuery, Result<FinancialYearSettingsDto>>
{
    public async Task<Result<FinancialYearSettingsDto>> Handle(GetFinancialYearSettingsQuery request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<FinancialYearSettingsDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var years = await db.AcademicYears.AsNoTracking()
            .Where(y => y.TenantId == tenant.TenantId.Value)
            .OrderByDescending(y => y.StartDate)
            .ToListAsync(ct);

        var current = years.FirstOrDefault(y => y.IsCurrent);
        var defaultYear = configuration["FinancialYear:DefaultYear"] ?? current?.Name;

        var dtos = years.Select(y => new FinancialYearDto(
            y.Id.ToString(),
            y.Name,
            y.StartDate.ToString("yyyy-MM-dd"),
            y.EndDate.ToString("yyyy-MM-dd"),
            y.IsCurrent)).ToList();

        return Result<FinancialYearSettingsDto>.Success(new FinancialYearSettingsDto(dtos, defaultYear, false));
    }
}

public sealed class SetCurrentFinancialYearCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<SetCurrentFinancialYearCommand, Result<FinancialYearSettingsDto>>
{
    public async Task<Result<FinancialYearSettingsDto>> Handle(SetCurrentFinancialYearCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<FinancialYearSettingsDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        var name = request.Request.Name.Trim();
        var years = await db.AcademicYears
            .Where(y => y.TenantId == tenant.TenantId.Value)
            .ToListAsync(ct);

        var target = years.FirstOrDefault(y => y.Name == name);
        if (target is null)
        {
            return Result<FinancialYearSettingsDto>.Failure(Error.NotFound("Financial year not found."));
        }

        foreach (var year in years)
        {
            year.IsCurrent = year.Id == target.Id;
        }

        await db.SaveChangesAsync(ct);

        var dtos = years.OrderByDescending(y => y.StartDate).Select(y => new FinancialYearDto(
            y.Id.ToString(),
            y.Name,
            y.StartDate.ToString("yyyy-MM-dd"),
            y.EndDate.ToString("yyyy-MM-dd"),
            y.IsCurrent)).ToList();

        return Result<FinancialYearSettingsDto>.Success(new FinancialYearSettingsDto(dtos, target.Name, false));
    }
}
