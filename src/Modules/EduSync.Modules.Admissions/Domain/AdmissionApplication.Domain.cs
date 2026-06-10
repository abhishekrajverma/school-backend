using EduSync.SharedKernel.Results;

namespace EduSync.Modules.Admissions.Domain;

public partial class AdmissionApplication
{
    public Result Submit()
    {
        if (Status != AdmissionStatuses.Draft)
        {
            return Result.Failure(Error.Conflict("Application is already submitted."));
        }

        Status = AdmissionStatuses.Submitted;
        CurrentStep = "review";
        SubmittedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result TransitionTo(string newStatus)
    {
        var status = newStatus.Trim().ToLowerInvariant();
        if (!AdmissionStatuses.All.Contains(status))
        {
            return Result.Failure(Error.Validation("Invalid admission status."));
        }

        if (!AdmissionStatuses.CanTransition(Status, status))
        {
            return Result.Failure(Error.Conflict($"Cannot transition from '{Status}' to '{status}'."));
        }

        Status = status;
        return Result.Success();
    }

    public Result Approve(Guid approvedByUserId, string? remarks)
    {
        var transition = TransitionTo(AdmissionStatuses.Approved);
        if (!transition.IsSuccess)
        {
            return transition;
        }

        Approvals.Add(new AdmissionApproval
        {
            Id = Guid.NewGuid(),
            AdmissionApplicationId = Id,
            ApprovedByUserId = approvedByUserId,
            Decision = AdmissionStatuses.Approved,
            Remarks = remarks,
            DecidedAt = DateTime.UtcNow,
        });

        return Result.Success();
    }
}
