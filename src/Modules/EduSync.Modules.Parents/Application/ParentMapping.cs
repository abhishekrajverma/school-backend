using System.Text.Json;
using EduSync.Modules.Parents.Application.Dtos;
using EduSync.Modules.Parents.Domain;

namespace EduSync.Modules.Parents.Application;

public static class ParentMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static ParentDto ToDto(Parent p) => new(
        p.ExternalId,
        p.FirstName,
        p.LastName,
        p.FullName,
        p.Email,
        p.Phone,
        p.Occupation,
        p.Address,
        ParseList(p.ChildrenJson),
        ParseList(p.StudentIdsJson),
        p.Status,
        p.AvatarUrl);

    public static string SerializeList(IReadOnlyList<string>? items) =>
        JsonSerializer.Serialize(items ?? Array.Empty<string>(), JsonOptions);

    private static IReadOnlyList<string> ParseList(string? json)
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
