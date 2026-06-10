using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Exams.Application;

public sealed record ExamDto(
    string Id,
    string ExamName,
    string ExamType,
    string Subject,
    string Class,
    string Date,
    string StartTime,
    int Duration,
    int TotalMarks,
    int PassingMarks,
    string? Room,
    string Status,
    int StudentsCount);

public sealed record CreateExamRequest(
    string ExamName,
    string ExamType,
    string Subject,
    string Class,
    string Date,
    string StartTime,
    int Duration,
    int TotalMarks,
    int PassingMarks,
    string? Room,
    string Status,
    int StudentsCount);

public sealed record UpdateExamRequest(
    string? ExamName,
    string? ExamType,
    string? Subject,
    string? Class,
    string? Date,
    string? StartTime,
    int? Duration,
    int? TotalMarks,
    int? PassingMarks,
    string? Room,
    string? Status,
    int? StudentsCount);

public sealed record ListExamsQuery(PaginationQuery Pagination, string? ClassName, string? Status)
    : IRequest<Result<PaginatedList<ExamDto>>>;

public sealed record GetExamByIdQuery(string ExternalId) : IRequest<Result<ExamDto>>;
public sealed record CreateExamCommand(CreateExamRequest Request) : IRequest<Result<ExamDto>>;
public sealed record UpdateExamCommand(string ExternalId, UpdateExamRequest Request) : IRequest<Result<ExamDto>>;
public sealed record DeleteExamCommand(string ExternalId) : IRequest<Result>;

public sealed record ExamResultDto(
    string Id,
    string ExamId,
    string StudentId,
    decimal MarksObtained,
    decimal TotalMarks,
    string? Grade,
    string Status,
    string? Remarks);

public sealed record RecordExamResultRequest(
    string ExamExternalId,
    string StudentExternalId,
    decimal MarksObtained,
    decimal TotalMarks,
    string? Grade,
    string? Remarks);

public sealed record ListExamResultsQuery(string? ExamExternalId, string? StudentExternalId, Guid? AcademicYearId)
    : IRequest<Result<IReadOnlyList<ExamResultDto>>>;
public sealed record RecordExamResultCommand(RecordExamResultRequest Request) : IRequest<Result<ExamResultDto>>;
