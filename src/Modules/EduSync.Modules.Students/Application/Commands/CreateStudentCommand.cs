using EduSync.Modules.Students.Application.Dtos;
using EduSync.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace EduSync.Modules.Students.Application.Commands;

public sealed record CreateStudentCommand(CreateStudentRequest Request) : IRequest<Result<StudentDto>>;

public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.Request.FirstName).NotEmpty();
        RuleFor(x => x.Request.LastName).NotEmpty();
        RuleFor(x => x.Request.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Request.Class).NotEmpty();
        RuleFor(x => x.Request.Section).NotEmpty();
        RuleFor(x => x.Request.AdmissionNo).NotEmpty();
    }
}
