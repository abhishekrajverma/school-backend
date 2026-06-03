using EduSync.SharedKernel.Results;
using MediatR;

namespace EduSync.Modules.Students.Application.Commands;

public sealed record DeleteStudentCommand(string ExternalId) : IRequest<Result>;
