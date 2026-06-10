namespace EduSync.Modules.Admissions.Application.Dtos;

public sealed record RegistrationListItemDto(
    string Id,
    string RegistrationNo,
    string Source,
    string Status,
    string ApplicantName,
    string? ClassSought,
    DateTime CreatedAt,
    DateTime? SubmittedAt);

public sealed record RegistrationDetailDto(
    string Id,
    string RegistrationNo,
    string Source,
    string Status,
    string ApplicantFirstName,
    string ApplicantLastName,
    string? ApplicantEmail,
    string? ApplicantPhone,
    string? ClassSought,
    string AcademicYearId,
    object FormData,
    DateTime CreatedAt,
    DateTime? SubmittedAt);

public sealed record CreateRegistrationRequest(
    string Source,
    string ApplicantFirstName,
    string ApplicantLastName,
    string? ApplicantEmail,
    string? ApplicantPhone,
    string? ClassSought,
    object? FormData);

public sealed record UpdateRegistrationRequest(
    string? ApplicantFirstName,
    string? ApplicantLastName,
    string? ApplicantEmail,
    string? ApplicantPhone,
    string? ClassSought,
    object? FormData);

public sealed record ConvertRegistrationToAdmissionRequest(string? Source);
