using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Staff.Application;

public sealed record TeacherAssignmentDto(
    string Id,
    string TeacherId,
    string AcademicYearId,
    string? ClassName,
    string? SubjectExternalId,
    string AssignmentType,
    bool IsActive);

public sealed record CreateTeacherAssignmentRequest(
    string TeacherExternalId,
    Guid AcademicYearId,
    string? ClassName,
    string? SubjectExternalId,
    string AssignmentType = "subject_teacher");

public sealed record ListTeacherAssignmentsQuery(string? TeacherExternalId, Guid? AcademicYearId)
    : IRequest<Result<IReadOnlyList<TeacherAssignmentDto>>>;
public sealed record CreateTeacherAssignmentCommand(CreateTeacherAssignmentRequest Request)
    : IRequest<Result<TeacherAssignmentDto>>;
public sealed record DeactivateTeacherAssignmentCommand(string ExternalId) : IRequest<Result>;
