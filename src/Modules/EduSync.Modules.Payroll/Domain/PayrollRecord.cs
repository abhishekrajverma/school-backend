using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Payroll.Domain;

public sealed class PayrollRecord : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string EmployeeExternalId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Hra { get; set; }
    public decimal Da { get; set; }
    public decimal Ta { get; set; }
    public decimal Medical { get; set; }
    public decimal Special { get; set; }
    public decimal PfDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal Insurance { get; set; }
    public decimal LoanDeduction { get; set; }
    public decimal OtherDeduction { get; set; }
    public decimal Bonus { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = "pending";
    public DateOnly? PaymentDate { get; set; }
}
