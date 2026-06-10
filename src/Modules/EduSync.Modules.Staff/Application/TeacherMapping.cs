using System.Text.Json;
using EduSync.Modules.Staff.Application.Dtos;
using EduSync.Modules.Staff.Domain;

namespace EduSync.Modules.Staff.Application;

public static class TeacherMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static TeacherDto ToDto(Teacher t) => new(
        t.ExternalId,
        t.FirstName,
        t.LastName,
        t.FullName,
        t.EmployeeId,
        t.Department,
        t.Subject,
        t.Qualification,
        t.ExperienceYears,
        t.Email,
        t.Phone,
        t.Salary,
        t.JoiningDate?.ToString("yyyy-MM-dd"),
        t.LifecycleStatus,
        ParseClasses(t.ClassesJson),
        t.AvatarUrl);

    public static string SerializeClasses(IReadOnlyList<string>? classes) =>
        JsonSerializer.Serialize(classes ?? Array.Empty<string>(), JsonOptions);

    private static IReadOnlyList<string> ParseClasses(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return Array.Empty<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
