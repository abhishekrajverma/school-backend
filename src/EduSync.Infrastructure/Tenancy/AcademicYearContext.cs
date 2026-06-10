namespace EduSync.Infrastructure.Tenancy;

public sealed class AcademicYearContext : IAcademicYearContext
{
    public Guid? AcademicYearId { get; private set; }
    public string? AcademicYearName { get; private set; }

    public void Set(Guid academicYearId, string name)
    {
        AcademicYearId = academicYearId;
        AcademicYearName = name;
    }
}
