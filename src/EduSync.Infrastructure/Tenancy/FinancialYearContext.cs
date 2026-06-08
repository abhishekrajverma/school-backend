namespace EduSync.Infrastructure.Tenancy;

public sealed class FinancialYearContext : IFinancialYearContext
{
    public string? FinancialYear { get; private set; }

    public void Set(string financialYear) => FinancialYear = financialYear;
}
