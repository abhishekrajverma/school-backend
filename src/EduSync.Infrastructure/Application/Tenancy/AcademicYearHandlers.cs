using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.Modules.Tenancy.Domain;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Tenancy;

public sealed class CreateAcademicYearCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreateAcademicYearCommand, Result<FinancialYearDto>>
{
    public async Task<Result<FinancialYearDto>> Handle(CreateAcademicYearCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<FinancialYearDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        if (!DateOnly.TryParse(request.Request.StartDate, out var start)
            || !DateOnly.TryParse(request.Request.EndDate, out var end))
        {
            return Result<FinancialYearDto>.Failure(Error.Validation("Invalid start or end date."));
        }

        var name = request.Request.Name.Trim();
        if (await db.AcademicYears.AnyAsync(y => y.TenantId == tenant.TenantId && y.Name == name, ct))
        {
            return Result<FinancialYearDto>.Failure(Error.Conflict("Academic year name already exists."));
        }

        var createResult = AcademicYear.Create(tenant.TenantId.Value, name, start, end, request.Request.SetAsCurrent);
        if (!createResult.IsSuccess)
        {
            return Result<FinancialYearDto>.Failure(createResult.Error!);
        }

        var year = createResult.Value!;
        if (request.Request.SetAsCurrent)
        {
            var existing = await db.AcademicYears
                .Where(y => y.TenantId == tenant.TenantId && y.IsCurrent)
                .ToListAsync(ct);
            foreach (var y in existing)
            {
                y.IsCurrent = false;
            }
        }

        db.AcademicYears.Add(year);
        await db.SaveChangesAsync(ct);

        return Result<FinancialYearDto>.Success(new FinancialYearDto(
            year.Id.ToString(),
            year.Name,
            year.StartDate.ToString("yyyy-MM-dd"),
            year.EndDate.ToString("yyyy-MM-dd"),
            year.IsCurrent));
    }
}

public sealed class CloseAcademicYearCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CloseAcademicYearCommand, Result<FinancialYearDto>>
{
    public async Task<Result<FinancialYearDto>> Handle(CloseAcademicYearCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue)
        {
            return Result<FinancialYearDto>.Failure(Error.Forbidden("Tenant context is required."));
        }

        if (!Guid.TryParse(request.AcademicYearId, out var yearId))
        {
            return Result<FinancialYearDto>.Failure(Error.Validation("Invalid academic year id."));
        }

        var year = await db.AcademicYears
            .FirstOrDefaultAsync(y => y.TenantId == tenant.TenantId && y.Id == yearId, ct);
        if (year is null)
        {
            return Result<FinancialYearDto>.Failure(Error.NotFound("Academic year not found."));
        }

        var closeResult = year.Close();
        if (!closeResult.IsSuccess)
        {
            return Result<FinancialYearDto>.Failure(closeResult.Error!);
        }

        await db.SaveChangesAsync(ct);
        return Result<FinancialYearDto>.Success(new FinancialYearDto(
            year.Id.ToString(),
            year.Name,
            year.StartDate.ToString("yyyy-MM-dd"),
            year.EndDate.ToString("yyyy-MM-dd"),
            year.IsCurrent));
    }
}
