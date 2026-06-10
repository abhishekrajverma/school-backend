using EduSync.SharedKernel.Results;

namespace EduSync.Modules.Admissions.Domain;

public partial class Registration
{
    public Result Submit()
    {
        if (Status != RegistrationStatuses.Draft)
        {
            return Result.Failure(Error.Conflict("Only draft registrations can be submitted."));
        }

        if (string.IsNullOrWhiteSpace(ApplicantFirstName))
        {
            return Result.Failure(Error.Validation("Applicant first name is required."));
        }

        Status = RegistrationStatuses.Submitted;
        SubmittedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Verify()
    {
        if (Status != RegistrationStatuses.Submitted)
        {
            return Result.Failure(Error.Conflict("Only submitted registrations can be verified."));
        }

        Status = RegistrationStatuses.Verified;
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status is RegistrationStatuses.Converted or RegistrationStatuses.Cancelled)
        {
            return Result.Failure(Error.Conflict("Registration cannot be cancelled."));
        }

        Status = RegistrationStatuses.Cancelled;
        return Result.Success();
    }

    public Result MarkConverted()
    {
        if (Status is RegistrationStatuses.Cancelled)
        {
            return Result.Failure(Error.Conflict("Cancelled registration cannot be converted."));
        }

        Status = RegistrationStatuses.Converted;
        return Result.Success();
    }
}
