using EduSync.Infrastructure.Tenancy;
using EduSync.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EduSync.Infrastructure.Middleware;

public sealed class FinancialYearResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IFinancialYearContext financialYearContext,
        IConfiguration configuration)
    {
        var header = context.Request.Headers[HttpHeaders.FinancialYear].FirstOrDefault()?.Trim();
        var defaultYear = configuration["FinancialYear:DefaultYear"] ?? FinancialYearDefaults.Demo;
        financialYearContext.Set(string.IsNullOrWhiteSpace(header) ? defaultYear : header);
        await next(context);
    }
}
