namespace EduSync.Modules.Academics.Application.Dtos;

public sealed record ClassDto(
    string Id,
    string Name,
    IReadOnlyList<string> Sections,
    int TotalStudents,
    string? ClassTeacher);

public sealed record SubjectDto(
    string Id,
    string Name,
    string Code,
    string Class,
    string? TeacherId,
    string? TeacherName,
    int WeeklyHours,
    string Status);

public sealed record CreateClassRequest(
    string Name,
    IReadOnlyList<string> Sections,
    int TotalStudents = 0,
    string? ClassTeacher = null);

public sealed record CreateSubjectRequest(
    string Name,
    string Code,
    string Class,
    string? TeacherId,
    string? TeacherName,
    int WeeklyHours,
    string Status = "active");
