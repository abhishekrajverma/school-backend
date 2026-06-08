namespace EduSync.Infrastructure.Tenancy;

public interface IFinancialYearContext
{
    string? FinancialYear { get; }
    bool IsResolved => !string.IsNullOrWhiteSpace(FinancialYear);
    void Set(string financialYear);
}
