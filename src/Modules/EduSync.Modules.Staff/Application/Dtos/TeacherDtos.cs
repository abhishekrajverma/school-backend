namespace EduSync.Modules.Staff.Application.Dtos;

public sealed record TeacherDto(
    string Id,
    string FirstName,
    string LastName,
    string Name,
    string EmployeeId,
    string Department,
    string Subject,
    string? Qualification,
    int Experience,
    string Email,
    string? Phone,
    decimal Salary,
    string? JoiningDate,
    string Status,
    IReadOnlyList<string> Classes,
    string? Avatar);

public sealed record CreateTeacherRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? DateOfBirth,
    string? Gender,
    string EmployeeId,
    string Department,
    string Subject,
    string Qualification,
    int Experience,
    string JoiningDate,
    decimal Salary,
    string? Address,
    string Status = "active",
    IReadOnlyList<string>? Classes = null);

public sealed record UpdateTeacherRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? EmployeeId,
    string? Department,
    string? Subject,
    string? Qualification,
    int? Experience,
    decimal? Salary,
    string? JoiningDate,
    string? Status,
    IReadOnlyList<string>? Classes);
