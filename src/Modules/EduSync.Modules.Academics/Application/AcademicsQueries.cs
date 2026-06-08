using EduSync.Modules.Academics.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Academics.Application;

public sealed record ListClassesQuery : IRequest<Result<IReadOnlyList<ClassDto>>>;
public sealed record ListSubjectsQuery(string? ClassName) : IRequest<Result<IReadOnlyList<SubjectDto>>>;
public sealed record CreateClassCommand(CreateClassRequest Request) : IRequest<Result<ClassDto>>;
public sealed record CreateSubjectCommand(CreateSubjectRequest Request) : IRequest<Result<SubjectDto>>;

public sealed record UpdateClassRequest(
    string? Name,
    IReadOnlyList<string>? Sections,
    int? TotalStudents,
    string? ClassTeacher);

public sealed record UpdateSubjectRequest(
    string? Name,
    string? Code,
    string? Class,
    string? TeacherId,
    string? TeacherName,
    int? WeeklyHours,
    string? Status);

public sealed record UpdateClassCommand(string ExternalId, UpdateClassRequest Request) : IRequest<Result<ClassDto>>;
public sealed record DeleteClassCommand(string ExternalId) : IRequest<Result>;
public sealed record UpdateSubjectCommand(string ExternalId, UpdateSubjectRequest Request) : IRequest<Result<SubjectDto>>;
public sealed record DeleteSubjectCommand(string ExternalId) : IRequest<Result>;
