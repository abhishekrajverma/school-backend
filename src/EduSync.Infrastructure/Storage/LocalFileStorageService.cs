using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Storage;

public sealed class LocalFileStorageService(IOptions<UploadOptions> options) : IFileStorageService
{
    private readonly UploadOptions _options = options.Value;

    public async Task<(string StoragePath, string RelativeUrl)> SaveAsync(
        Guid tenantId,
        string externalId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        var tenantDir = Path.Combine(_options.RootPath, tenantId.ToString("N"));
        Directory.CreateDirectory(tenantDir);
        var storagePath = Path.Combine(tenantDir, $"{externalId}{ext}");
        await using var file = File.Create(storagePath);
        await content.CopyToAsync(file, cancellationToken);
        var relativeUrl = $"/api/uploads/{externalId}/download";
        return (storagePath, relativeUrl);
    }

    public Task<(Stream Stream, string ContentType, bool Dispose)> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(storagePath))
        {
            throw new FileNotFoundException("Stored file not found.", storagePath);
        }

        var stream = File.OpenRead(storagePath);
        var contentType = GetContentType(Path.GetExtension(storagePath));
        return Task.FromResult<(Stream, string, bool)>((stream, contentType, true));
    }

    private static string GetContentType(string ext) => ext.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream",
    };
}
