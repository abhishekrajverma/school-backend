using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace EduSync.Infrastructure.Storage;

/// <summary>AWS S3 or MinIO-compatible object storage.</summary>
public sealed class S3CompatibleFileStorageService(IOptions<UploadOptions> options) : IFileStorageService, IDisposable
{
    private readonly UploadOptions _options = options.Value;
    private IAmazonS3? _client;

    private IAmazonS3 Client
    {
        get
        {
            if (_client is not null)
            {
                return _client;
            }

            if (string.IsNullOrWhiteSpace(_options.S3AccessKey) || string.IsNullOrWhiteSpace(_options.S3SecretKey))
            {
                throw new InvalidOperationException("Uploads:S3AccessKey and S3SecretKey are required when Provider is S3.");
            }

            var config = new AmazonS3Config
            {
                ForcePathStyle = true,
                AuthenticationRegion = _options.S3Region,
            };

            if (!string.IsNullOrWhiteSpace(_options.S3ServiceUrl))
            {
                config.ServiceURL = _options.S3ServiceUrl;
            }
            else
            {
                config.RegionEndpoint = RegionEndpoint.GetBySystemName(_options.S3Region);
            }

            _client = new AmazonS3Client(_options.S3AccessKey, _options.S3SecretKey, config);
            return _client;
        }
    }

    public async Task<(string StoragePath, string RelativeUrl)> SaveAsync(
        Guid tenantId,
        string externalId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var ext = Path.GetExtension(fileName);
        var key = $"{tenantId:N}/{externalId}{ext}";
        var request = new PutObjectRequest
        {
            BucketName = _options.S3Bucket,
            Key = key,
            InputStream = content,
            AutoCloseStream = false,
        };
        await Client.PutObjectAsync(request, cancellationToken);
        return (key, $"/api/uploads/{externalId}/download");
    }

    public async Task<(Stream Stream, string ContentType, bool Dispose)> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await Client.GetObjectAsync(_options.S3Bucket, storagePath, cancellationToken);
            var contentType = response.Headers.ContentType ?? "application/octet-stream";
            return (response.ResponseStream, contentType, true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException("Object not found.", storagePath);
        }
    }

    public void Dispose() => _client?.Dispose();
}
