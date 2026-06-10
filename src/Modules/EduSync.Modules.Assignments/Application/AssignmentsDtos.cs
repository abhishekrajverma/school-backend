using EduSync.SharedKernel.Pagination;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Assignments.Application;

public sealed record AssignmentDto(
    string Id,
    string Title,
    string? Description,
    string ClassName,
    string? Section,
    string? Subject,
    string DueDate,
    string Status,
    string AcademicYearId);

public sealed record StudentAssignmentDto(
    string Id,
    string AssignmentId,
    string Title,
    string DueDate,
    string Status,
    string? SubmissionText,
    decimal? Score,
    DateTime? SubmittedAt);

public sealed record CreateAssignmentRequest(
    string Title,
    string? Description,
    string ClassName,
    string? Section,
    string? Subject,
    string DueDate,
    string? TeacherExternalId);

public sealed record SubmitAssignmentRequest(string SubmissionText);

public sealed record ListAssignmentsQuery(PaginationQuery Pagination, string? ClassName)
    : IRequest<Result<PaginatedList<AssignmentDto>>>;
public sealed record CreateAssignmentCommand(CreateAssignmentRequest Request) : IRequest<Result<AssignmentDto>>;
public sealed record ListStudentAssignmentsQuery : IRequest<Result<IReadOnlyList<StudentAssignmentDto>>>;
public sealed record SubmitStudentAssignmentCommand(string AssignmentExternalId, SubmitAssignmentRequest Request)
    : IRequest<Result<StudentAssignmentDto>>;
