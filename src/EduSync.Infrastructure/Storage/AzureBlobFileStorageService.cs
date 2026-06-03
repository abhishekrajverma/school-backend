using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Storage;

public sealed class AzureBlobFileStorageService(IOptions<UploadOptions> options) : IFileStorageService
{
    private readonly UploadOptions _options = options.Value;

    private BlobContainerClient GetContainer()
    {
        if (string.IsNullOrWhiteSpace(_options.AzureConnectionString))
        {
            throw new InvalidOperationException("Uploads:AzureConnectionString is required when Provider is Azure.");
        }

        var client = new BlobContainerClient(_options.AzureConnectionString, _options.AzureContainer);
        client.CreateIfNotExists(PublicAccessType.None);
        return client;
    }

    public async Task<(string StoragePath, string RelativeUrl)> SaveAsync(
        Guid tenantId,
        string externalId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        var blobPath = $"{tenantId:N}/{externalId}{ext}";
        var container = GetContainer();
        var blob = container.GetBlobClient(blobPath);
        await blob.UploadAsync(content, overwrite: true, cancellationToken);
        return (blobPath, $"/api/uploads/{externalId}/download");
    }

    public async Task<(Stream Stream, string ContentType, bool Dispose)> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var container = GetContainer();
        var blob = container.GetBlobClient(storagePath);
        if (!await blob.ExistsAsync(cancellationToken))
        {
            throw new FileNotFoundException("Blob not found.", storagePath);
        }

        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
        return (response.Value.Content, contentType, true);
    }
}
