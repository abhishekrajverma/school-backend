using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Fees.Domain;

public sealed class FeeInvoice : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string FinancialYear { get; set; } = string.Empty;
    public string InvoiceNo { get; set; } = string.Empty;
    public string StudentExternalId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string FeeType { get; set; } = "tuition";
    public decimal TotalFee { get; set; }
    public decimal Paid { get; set; }
    public decimal Pending { get; set; }
    public decimal Discount { get; set; }
    public decimal Fine { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public string Status { get; set; } = "pending";
    public string? PaymentMethod { get; set; }
    public string? FeeItemsJson { get; set; }
}
