namespace EduSync.Modules.Admissions.Application.Dtos;

public sealed record AdmissionListItemDto(
    string Id,
    string ApplicationNo,
    string Status,
    string Source,
    string? RegistrationId,
    string CurrentStep,
    string? ApplicantName,
    string? ClassSought,
    string? AcademicSession,
    DateTime CreatedAt,
    DateTime? SubmittedAt);

public sealed record AdmissionDetailDto(
    string Id,
    string ApplicationNo,
    string Status,
    string Source,
    string? RegistrationId,
    string? ApprovedStudentId,
    string CurrentStep,
    object FormData,
    object? Documents,
    string? ApplicantName,
    string? ClassSought,
    string? AcademicSession,
    DateTime CreatedAt,
    DateTime? SubmittedAt);

public sealed record ApproveAdmissionRequest(string? Remarks);

public sealed record CreateAdmissionRequest(
    string? CurrentStep,
    object FormData);

public sealed record UpdateAdmissionRequest(
    string? CurrentStep,
    object? FormData);

public sealed record UpdateAdmissionStatusRequest(string Status);

public sealed record RegisterAdmissionDocumentRequest(
    string DocumentType,
    string FileName,
    string ContentType,
    long Size,
    string? StorageUrl);

public sealed record AdmissionDocumentDto(
    string DocumentType,
    string FileName,
    string ContentType,
    long Size,
    string? StorageUrl,
    DateTime UploadedAt);
