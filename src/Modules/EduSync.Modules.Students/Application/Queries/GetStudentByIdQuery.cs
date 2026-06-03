using EduSync.Modules.Students.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Students.Application.Queries;

public sealed record GetStudentByIdQuery(string ExternalId) : IRequest<Result<StudentDto>>;
