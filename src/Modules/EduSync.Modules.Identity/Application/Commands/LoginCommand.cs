using EduSync.Modules.Identity.Application.Dtos;
using EduSync.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace EduSync.Modules.Identity.Application.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
