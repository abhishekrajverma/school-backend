using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Uploads.Application;

public sealed record UploadFileDto(string Id, string Url, string FileName, string ContentType, long Size);

public sealed record UploadFileCommand(
    Stream Content,
    string FileName,
    string ContentType,
    long SizeBytes,
    string? Category,
    Guid? UploadedByUserId) : IRequest<Result<UploadFileDto>>;

public sealed record GetUploadByIdQuery(string ExternalId) : IRequest<Result<UploadFileDto>>;

public sealed record DownloadUploadQuery(string ExternalId) : IRequest<Result<DownloadFileResult>>;

public sealed record DownloadFileResult(
    Stream Stream,
    string FileName,
    string ContentType,
    bool DisposeStream);
