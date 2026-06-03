using EduSync.Modules.Tenancy.Application.Dtos;
using EduSync.SharedKernel.Results;
using FluentValidation;
using MediatR;

namespace EduSync.Modules.Tenancy.Application.Commands;

public sealed record ProvisionTenantCommand(
    string SchoolName,
    string Slug,
    string AdminEmail,
    string AdminPassword,
    string AdminName,
    string PlanId) : IRequest<Result<ProvisionTenantResponse>>;

public sealed class ProvisionTenantCommandValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantCommandValidator()
    {
        RuleFor(x => x.SchoolName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(64).Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$");
        RuleFor(x => x.AdminEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.AdminPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.AdminName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.PlanId).NotEmpty();
    }
}
