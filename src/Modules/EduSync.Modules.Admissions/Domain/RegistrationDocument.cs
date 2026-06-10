namespace EduSync.Modules.Admissions.Domain;

public sealed class RegistrationDocument
{
    public Guid Id { get; set; }
    public Guid RegistrationId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? StorageUrl { get; set; }
    public DateTime UploadedAt { get; set; }

    public Registration Registration { get; set; } = null!;
}
