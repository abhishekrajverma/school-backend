using EduSync.SharedKernel.Results;

namespace EduSync.Modules.Tenancy.Domain;

public partial class AcademicYear
{
    public bool IsClosed => EndDate < DateOnly.FromDateTime(DateTime.UtcNow);

    public static Result<AcademicYear> Create(
        Guid tenantId,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        bool isCurrent = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<AcademicYear>.Failure(Error.Validation("Academic year name is required."));
        }

        if (endDate <= startDate)
        {
            return Result<AcademicYear>.Failure(Error.Validation("End date must be after start date."));
        }

        return Result<AcademicYear>.Success(new AcademicYear
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsCurrent = isCurrent,
        });
    }

    public Result Close()
    {
        if (IsClosed)
        {
            return Result.Failure(Error.Conflict("Academic year is already closed."));
        }

        EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        IsCurrent = false;
        return Result.Success();
    }

    public Result SetAsCurrent()
    {
        if (IsClosed)
        {
            return Result.Failure(Error.Conflict("Cannot set a closed academic year as current."));
        }

        IsCurrent = true;
        return Result.Success();
    }
}
