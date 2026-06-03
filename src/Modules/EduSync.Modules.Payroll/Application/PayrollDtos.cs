using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Payroll.Application;

public sealed record PayrollDto(
    string Id,
    string EmployeeId,
    string EmployeeName,
    string Department,
    string Month,
    int Year,
    decimal BasicSalary,
    decimal Hra,
    decimal Da,
    decimal Ta,
    decimal Medical,
    decimal Special,
    decimal PfDeduction,
    decimal TaxDeduction,
    decimal Insurance,
    decimal LoanDeduction,
    decimal OtherDeduction,
    decimal Bonus,
    decimal GrossSalary,
    decimal TotalDeductions,
    decimal NetSalary,
    string Status,
    string? PaymentDate);

public sealed record CreatePayrollRequest(
    string EmployeeId,
    string EmployeeName,
    string Department,
    string Month,
    int Year,
    decimal BasicSalary,
    decimal Hra,
    decimal Da,
    decimal Ta,
    decimal Medical,
    decimal Special,
    decimal PfDeduction,
    decimal TaxDeduction,
    decimal Insurance,
    decimal LoanDeduction,
    decimal OtherDeduction,
    decimal Bonus);

public sealed record ListPayrollQuery(PaginationQuery Pagination, string? Month, int? Year, string? Status, string? EmployeeId)
    : IRequest<Result<PaginatedList<PayrollDto>>>;

public sealed record GetPayrollByIdQuery(string ExternalId) : IRequest<Result<PayrollDto>>;
public sealed record CreatePayrollCommand(CreatePayrollRequest Request) : IRequest<Result<PayrollDto>>;
public sealed record ProcessPayrollCommand(string ExternalId) : IRequest<Result<PayrollDto>>;
