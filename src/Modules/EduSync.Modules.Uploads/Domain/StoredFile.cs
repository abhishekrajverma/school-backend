using EduSync.SharedKernel.Entities;

namespace EduSync.Modules.Uploads.Domain;

public sealed class StoredFile : TenantEntity
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? Category { get; set; }
    public Guid? UploadedByUserId { get; set; }
}
