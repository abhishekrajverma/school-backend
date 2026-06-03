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
