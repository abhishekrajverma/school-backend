using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduSync.Infrastructure.Middleware;

public sealed class AcademicYearResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        IAcademicYearContext academicYearContext,
        IFinancialYearContext financialYearContext,
        EduSyncDbContext db,
        IConfiguration configuration)
    {
        if (!tenantContext.TenantId.HasValue)
        {
            await SetFinancialYearFallbackAsync(financialYearContext, configuration, context);
            await next(context);
            return;
        }

        var yearIdHeader = context.Request.Headers[HttpHeaders.AcademicYearId].FirstOrDefault()?.Trim();
        var yearNameHeader = context.Request.Headers[HttpHeaders.FinancialYear].FirstOrDefault()?.Trim();

        Modules.Tenancy.Domain.AcademicYear? year = null;

        if (!string.IsNullOrWhiteSpace(yearIdHeader) && Guid.TryParse(yearIdHeader, out var yearId))
        {
            year = await db.AcademicYears.AsNoTracking()
                .FirstOrDefaultAsync(y => y.TenantId == tenantContext.TenantId && y.Id == yearId, context.RequestAborted);
        }
        else if (!string.IsNullOrWhiteSpace(yearNameHeader))
        {
            year = await db.AcademicYears.AsNoTracking()
                .FirstOrDefaultAsync(
                    y => y.TenantId == tenantContext.TenantId && y.Name == yearNameHeader,
                    context.RequestAborted);
        }
        else
        {
            year = await db.AcademicYears.AsNoTracking()
                .FirstOrDefaultAsync(
                    y => y.TenantId == tenantContext.TenantId && y.IsCurrent,
                    context.RequestAborted);
        }

        if (year is not null)
        {
            academicYearContext.Set(year.Id, year.Name);
            financialYearContext.Set(year.Name);
        }
        else
        {
            await SetFinancialYearFallbackAsync(financialYearContext, configuration, context);
        }

        await next(context);
    }

    private static Task SetFinancialYearFallbackAsync(
        IFinancialYearContext financialYearContext,
        IConfiguration configuration,
        HttpContext context)
    {
        var header = context.Request.Headers[HttpHeaders.FinancialYear].FirstOrDefault()?.Trim();
        var defaultYear = configuration["FinancialYear:DefaultYear"] ?? FinancialYearDefaults.Demo;
        financialYearContext.Set(string.IsNullOrWhiteSpace(header) ? defaultYear : header);
        return Task.CompletedTask;
    }
}
