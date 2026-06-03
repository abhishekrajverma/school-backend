using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Inventory.Domain;

public sealed class InventoryItem : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStock { get; set; }
    public string Unit { get; set; } = "pcs";
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = "in-stock";
    public DateOnly LastRestocked { get; set; }
}
