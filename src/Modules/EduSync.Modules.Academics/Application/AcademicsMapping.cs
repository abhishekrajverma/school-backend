using System.Text.Json;
using EduSync.Modules.Academics.Application.Dtos;
using EduSync.Modules.Academics.Domain;

namespace EduSync.Modules.Academics.Application;

public static class AcademicsMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static ClassDto ToDto(SchoolClass c) => new(
        c.ExternalId,
        c.Name,
        ParseSections(c.SectionsJson),
        c.TotalStudents,
        c.ClassTeacherName);

    public static SubjectDto ToDto(Subject s) => new(
        s.ExternalId,
        s.Name,
        s.Code,
        s.ClassName,
        s.TeacherExternalId,
        s.TeacherName,
        s.WeeklyHours,
        s.Status);

    public static string SerializeSections(IReadOnlyList<string> sections) =>
        JsonSerializer.Serialize(sections, JsonOptions);

    private static IReadOnlyList<string> ParseSections(string json)
    {
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
