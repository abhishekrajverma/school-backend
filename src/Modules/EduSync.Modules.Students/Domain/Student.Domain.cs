using EduSync.SharedKernel.Constants;
using EduSync.SharedKernel.Results;

namespace EduSync.Modules.Students.Domain;

public partial class Student
{
    public Result SetLifecycleStatus(string status)
    {
        if (!LifecycleStatuses.All.Contains(status))
        {
            return Result.Failure(Error.Validation("Invalid lifecycle status."));
        }

        LifecycleStatus = status;
        return Result.Success();
    }

    public bool CanBePromoted() => LifecycleStatuses.IsPromotable(LifecycleStatus);

    public Result MarkInactive() => SetLifecycleStatus(LifecycleStatuses.Inactive);

    public Result MarkActive() => SetLifecycleStatus(LifecycleStatuses.Active);
}
