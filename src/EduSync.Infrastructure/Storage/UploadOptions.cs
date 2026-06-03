namespace EduSync.Infrastructure.Storage;

public sealed class UploadOptions
{
    /// <summary>Local, Azure, or S3-compatible (MinIO, AWS S3).</summary>
    public string Provider { get; set; } = "Local";
    public string RootPath { get; set; } = "uploads";
    public long MaxBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".csv"];
    public string? AzureConnectionString { get; set; }
    public string AzureContainer { get; set; } = "edusync-uploads";
    public string? S3ServiceUrl { get; set; }
    public string? S3AccessKey { get; set; }
    public string? S3SecretKey { get; set; }
    public string S3Bucket { get; set; } = "edusync-uploads";
    public string S3Region { get; set; } = "us-east-1";
}
