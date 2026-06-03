using EduSync.Infrastructure.Pagination;
using EduSync.Infrastructure.Persistence;
using EduSync.Infrastructure.Tenancy;
using EduSync.Modules.Payroll.Application;
using EduSync.Modules.Payroll.Domain;
using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Payroll;

internal static class PayrollMapping
{
    public static PayrollDto ToDto(PayrollRecord r) => new(
        r.ExternalId, r.EmployeeExternalId, r.EmployeeName, r.Department, r.Month, r.Year,
        r.BasicSalary, r.Hra, r.Da, r.Ta, r.Medical, r.Special,
        r.PfDeduction, r.TaxDeduction, r.Insurance, r.LoanDeduction, r.OtherDeduction, r.Bonus,
        r.GrossSalary, r.TotalDeductions, r.NetSalary, r.Status, r.PaymentDate?.ToString("yyyy-MM-dd"));

    public static void Recalculate(PayrollRecord r)
    {
        r.GrossSalary = r.BasicSalary + r.Hra + r.Da + r.Ta + r.Medical + r.Special + r.Bonus;
        r.TotalDeductions = r.PfDeduction + r.TaxDeduction + r.Insurance + r.LoanDeduction + r.OtherDeduction;
        r.NetSalary = r.GrossSalary - r.TotalDeductions;
    }
}

public sealed class ListPayrollQueryHandler(EduSyncDbContext db)
    : IRequestHandler<ListPayrollQuery, Result<PaginatedList<PayrollDto>>>
{
    public async Task<Result<PaginatedList<PayrollDto>>> Handle(ListPayrollQuery request, CancellationToken ct)
    {
        var query = db.PayrollRecords.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Month)) query = query.Where(x => x.Month == request.Month);
        if (request.Year.HasValue) query = query.Where(x => x.Year == request.Year);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (!string.IsNullOrWhiteSpace(request.EmployeeId)) query = query.Where(x => x.EmployeeExternalId == request.EmployeeId);
        query = query.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month);
        var page = await QueryPagination.ToPaginatedListAsync(query, request.Pagination, ct);
        var items = page.Items.Select(PayrollMapping.ToDto).ToList();
        return Result<PaginatedList<PayrollDto>>.Success(
            PaginatedList<PayrollDto>.Create(items, page.Page, page.PageSize, page.TotalCount));
    }
}

public sealed class GetPayrollByIdQueryHandler(EduSyncDbContext db)
    : IRequestHandler<GetPayrollByIdQuery, Result<PayrollDto>>
{
    public async Task<Result<PayrollDto>> Handle(GetPayrollByIdQuery request, CancellationToken ct)
    {
        var r = await db.PayrollRecords.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        return r is null ? Result<PayrollDto>.Failure(Error.NotFound("Payroll record not found."))
            : Result<PayrollDto>.Success(PayrollMapping.ToDto(r));
    }
}

public sealed class CreatePayrollCommandHandler(EduSyncDbContext db, ITenantContext tenant)
    : IRequestHandler<CreatePayrollCommand, Result<PayrollDto>>
{
    public async Task<Result<PayrollDto>> Handle(CreatePayrollCommand request, CancellationToken ct)
    {
        if (!tenant.TenantId.HasValue) return Result<PayrollDto>.Failure(Error.Forbidden("Tenant required."));
        var b = request.Request;
        var record = new PayrollRecord
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.TenantId.Value,
            ExternalId = Guid.NewGuid().ToString("N")[..8],
            EmployeeExternalId = b.EmployeeId,
            EmployeeName = b.EmployeeName,
            Department = b.Department,
            Month = b.Month,
            Year = b.Year,
            BasicSalary = b.BasicSalary,
            Hra = b.Hra, Da = b.Da, Ta = b.Ta, Medical = b.Medical, Special = b.Special,
            PfDeduction = b.PfDeduction, TaxDeduction = b.TaxDeduction, Insurance = b.Insurance,
            LoanDeduction = b.LoanDeduction, OtherDeduction = b.OtherDeduction, Bonus = b.Bonus,
            Status = "pending",
        };
        PayrollMapping.Recalculate(record);
        db.PayrollRecords.Add(record);
        await db.SaveChangesAsync(ct);
        return Result<PayrollDto>.Success(PayrollMapping.ToDto(record));
    }
}

public sealed class ProcessPayrollCommandHandler(EduSyncDbContext db)
    : IRequestHandler<ProcessPayrollCommand, Result<PayrollDto>>
{
    public async Task<Result<PayrollDto>> Handle(ProcessPayrollCommand request, CancellationToken ct)
    {
        var r = await db.PayrollRecords.FirstOrDefaultAsync(x => x.ExternalId == request.ExternalId && !x.IsDeleted, ct);
        if (r is null) return Result<PayrollDto>.Failure(Error.NotFound("Payroll record not found."));
        r.Status = r.Status switch
        {
            "pending" => "approved",
            "approved" => "paid",
            _ => r.Status,
        };
        if (r.Status == "paid") r.PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Result<PayrollDto>.Success(PayrollMapping.ToDto(r));
    }
}
