using EduSync.Modules.Academics.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Academics.Application;

public sealed record ListClassesQuery : IRequest<Result<IReadOnlyList<ClassDto>>>;
public sealed record ListSubjectsQuery(string? ClassName) : IRequest<Result<IReadOnlyList<SubjectDto>>>;
public sealed record CreateClassCommand(CreateClassRequest Request) : IRequest<Result<ClassDto>>;
public sealed record CreateSubjectCommand(CreateSubjectRequest Request) : IRequest<Result<SubjectDto>>;
