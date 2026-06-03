using EduSync.Infrastructure.Security;
using EduSync.Modules.Students.Application;
using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Domain;

namespace EduSync.Infrastructure.Application.Students;

public static class StudentSensitiveFields
{
    public static void ApplyEncryption(Student student, IFieldEncryptionService encryption)
    {
        if (!encryption.IsEnabled)
        {
            return;
        }

        student.Email = encryption.Encrypt(student.Email);
        student.Phone = encryption.Encrypt(student.Phone);
        student.ParentEmail = encryption.Encrypt(student.ParentEmail);
        student.ParentPhone = encryption.Encrypt(student.ParentPhone);
        student.Address = encryption.Encrypt(student.Address);
    }

    public static StudentDto ToDto(Student student, IFieldEncryptionService encryption) => new(
        student.ExternalId,
        student.FirstName,
        student.LastName,
        student.FullName,
        student.ClassName,
        student.Section,
        student.RollNo,
        student.AdmissionNo,
        encryption.Decrypt(student.Email) ?? student.Email,
        encryption.Decrypt(student.Phone),
        student.DateOfBirth?.ToString("yyyy-MM-dd"),
        student.Gender,
        student.BloodGroup,
        encryption.Decrypt(student.Address),
        student.ParentName,
        encryption.Decrypt(student.ParentPhone),
        encryption.Decrypt(student.ParentEmail),
        student.Status,
        student.FeeStatus,
        student.AttendancePercent,
        student.AvatarUrl);
}
