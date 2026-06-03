using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Library.Domain;

public sealed class Book : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int? PublishYear { get; set; }
    public int Quantity { get; set; }
    public int Available { get; set; }
    public int Issued { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}
