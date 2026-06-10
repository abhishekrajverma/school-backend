using EduSync.Infrastructure.Security;
using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Domain;

namespace EduSync.Infrastructure.Application.Students;

public static class StudentProjection
{
    public static StudentDto ToDto(
        Student student,
        StudentEnrollment? enrollment,
        string? parentName,
        string? parentPhone,
        string? parentEmail,
        IFieldEncryptionService encryption,
        string? feeStatus = null,
        int attendance = 0) => new(
        student.ExternalId,
        student.FirstName,
        student.LastName,
        student.FullName,
        enrollment?.ClassName ?? string.Empty,
        enrollment?.Section ?? string.Empty,
        enrollment?.RollNo ?? string.Empty,
        student.AdmissionNo,
        encryption.Decrypt(student.Email) ?? student.Email,
        encryption.Decrypt(student.Phone),
        student.DateOfBirth?.ToString("yyyy-MM-dd"),
        student.Gender,
        student.BloodGroup,
        encryption.Decrypt(student.Address),
        parentName,
        encryption.Decrypt(parentPhone),
        encryption.Decrypt(parentEmail),
        student.LifecycleStatus,
        feeStatus,
        attendance,
        student.AvatarUrl);
}
