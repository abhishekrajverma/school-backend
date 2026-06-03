using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Fees.Domain;

public sealed class FeePayment : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid FeeInvoiceId { get; set; }
    public string StudentExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? Remarks { get; set; }
    public DateTime PaidAt { get; set; }

    public FeeInvoice Invoice { get; set; } = null!;
}
