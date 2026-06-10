using System.Text.Json;
using EduSync.Modules.Admissions.Application.Dtos;
using EduSync.Modules.Admissions.Domain;

namespace EduSync.Infrastructure.Application.Admissions;

internal static class AdmissionJsonHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static string SerializeForm(object formData) =>
        JsonSerializer.Serialize(formData, JsonOptions);

    public static object ParseForm(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions)
                   ?? new Dictionary<string, JsonElement>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public static object? ParseDocuments(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<List<AdmissionDocumentDto>>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static void ApplyFormMetadata(AdmissionApplication app, string formJson)
    {
        app.FormDataJson = formJson;
        try
        {
            using var doc = JsonDocument.Parse(formJson);
            var root = doc.RootElement;
            var first = GetString(root, "firstName");
            var last = GetString(root, "lastName");
            app.ApplicantName = string.Join(' ', new[] { first, last }.Where(s => !string.IsNullOrWhiteSpace(s)));
            app.ClassSought = GetString(root, "classSought");
            app.AcademicSession = GetString(root, "academicSession");
        }
        catch
        {
            // keep existing metadata
        }
    }

    public static AdmissionListItemDto ToListItem(AdmissionApplication a) => new(
        a.ExternalId,
        a.ApplicationNo,
        a.Status,
        a.Source,
        a.RegistrationId?.ToString(),
        a.CurrentStep,
        a.ApplicantName,
        a.ClassSought,
        a.AcademicSession,
        a.CreatedAt,
        a.SubmittedAt);

    public static AdmissionDetailDto ToDetail(AdmissionApplication a) => new(
        a.ExternalId,
        a.ApplicationNo,
        a.Status,
        a.Source,
        a.RegistrationId?.ToString(),
        a.ApprovedStudentExternalId,
        a.CurrentStep,
        ParseForm(a.FormDataJson),
        ParseDocuments(a.DocumentsJson),
        a.ApplicantName,
        a.ClassSought,
        a.AcademicSession,
        a.CreatedAt,
        a.SubmittedAt);

    public static string AppendDocument(string? existingJson, AdmissionDocumentDto doc)
    {
        var list = new List<AdmissionDocumentDto>();
        if (!string.IsNullOrWhiteSpace(existingJson))
        {
            try
            {
                list = JsonSerializer.Deserialize<List<AdmissionDocumentDto>>(existingJson, JsonOptions) ?? [];
            }
            catch
            {
                list = [];
            }
        }

        list.RemoveAll(d => string.Equals(d.DocumentType, doc.DocumentType, StringComparison.OrdinalIgnoreCase));
        list.Add(doc);
        return JsonSerializer.Serialize(list, JsonOptions);
    }

    public static (string FirstName, string LastName, string Email, string? Phone, string? Section, string? RollNo) ParseStudentFields(string formJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(formJson);
            var root = doc.RootElement;
            return (
                GetString(root, "firstName") ?? "Student",
                GetString(root, "lastName") ?? string.Empty,
                GetString(root, "email") ?? $"{Guid.NewGuid():N}@student.local",
                GetString(root, "phone"),
                GetString(root, "section") ?? "A",
                GetString(root, "rollNo") ?? "0");
        }
        catch
        {
            return ("Student", string.Empty, $"{Guid.NewGuid():N}@student.local", null, "A", "0");
        }
    }

    private static string? GetString(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;
        if (!root.TryGetProperty(name, out var prop))
        {
            var pascal = char.ToUpperInvariant(name[0]) + name[1..];
            if (!root.TryGetProperty(pascal, out prop)) return null;
        }

        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }
}
