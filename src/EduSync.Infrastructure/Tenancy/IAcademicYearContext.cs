namespace EduSync.Infrastructure.Tenancy;

public interface IAcademicYearContext
{
    Guid? AcademicYearId { get; }
    string? AcademicYearName { get; }
    bool IsResolved => AcademicYearId.HasValue;
    void Set(Guid academicYearId, string name);
}
