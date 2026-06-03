using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Imports.Application;

public sealed record ImportRowError(int Line, string Message);

public sealed record ImportResultDto(
    int Imported,
    int Skipped,
    int Failed,
    IReadOnlyList<ImportRowError> Errors);

public sealed record ImportStudentsCsvCommand(Stream CsvStream) : IRequest<Result<ImportResultDto>>;
public sealed record ImportTeachersCsvCommand(Stream CsvStream) : IRequest<Result<ImportResultDto>>;
public sealed record QueueImportStudentsCommand(string StoredFileExternalId) : IRequest<Result<string>>;
public sealed record QueueImportTeachersCommand(string StoredFileExternalId) : IRequest<Result<string>>;
