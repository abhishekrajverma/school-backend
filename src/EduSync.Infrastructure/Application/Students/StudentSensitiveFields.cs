using EduSync.Infrastructure.Security;
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
        student.Address = encryption.Encrypt(student.Address);
    }

    public static StudentDto ToDto(
        Student student,
        StudentEnrollment? enrollment,
        IFieldEncryptionService encryption,
        string? parentName = null,
        string? parentPhone = null,
        string? parentEmail = null,
        string? feeStatus = null,
        int attendance = 0) =>
        StudentProjection.ToDto(
            student,
            enrollment,
            parentName,
            parentPhone,
            parentEmail,
            encryption,
            feeStatus,
            attendance);
}
