namespace EduSync.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<(string StoragePath, string RelativeUrl)> SaveAsync(
        Guid tenantId,
        string externalId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<(Stream Stream, string ContentType, bool Dispose)> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}
