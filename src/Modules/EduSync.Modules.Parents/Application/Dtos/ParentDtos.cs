namespace EduSync.Modules.Parents.Application.Dtos;

public sealed record ParentDto(
    string Id,
    string FirstName,
    string LastName,
    string Name,
    string Email,
    string? Phone,
    string? Occupation,
    string? Address,
    IReadOnlyList<string> Children,
    IReadOnlyList<string> StudentIds,
    string Status,
    string? Avatar);

public sealed record CreateParentRequest(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Occupation,
    string? Address,
    IReadOnlyList<string>? Children,
    IReadOnlyList<string>? StudentIds,
    string Status = "active");

public sealed record UpdateParentRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Occupation,
    string? Address,
    IReadOnlyList<string>? Children,
    IReadOnlyList<string>? StudentIds,
    string? Status);
