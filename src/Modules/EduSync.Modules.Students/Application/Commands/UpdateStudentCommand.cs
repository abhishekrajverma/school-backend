using EduSync.Modules.Students.Application.Dtos;
using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Students.Application.Commands;

public sealed record UpdateStudentCommand(string ExternalId, UpdateStudentRequest Request) : IRequest<Result<StudentDto>>;
