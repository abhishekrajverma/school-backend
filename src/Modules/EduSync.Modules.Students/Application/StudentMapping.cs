using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Domain;

namespace EduSync.Modules.Students.Application;

public static class StudentMapping
{
    public static StudentDto ToDto(Student student, StudentEnrollment? enrollment = null) => new(
        student.ExternalId,
        student.FirstName,
        student.LastName,
        student.FullName,
        enrollment?.ClassName ?? string.Empty,
        enrollment?.Section ?? string.Empty,
        enrollment?.RollNo ?? string.Empty,
        student.AdmissionNo,
        student.Email,
        student.Phone,
        student.DateOfBirth?.ToString("yyyy-MM-dd"),
        student.Gender,
        student.BloodGroup,
        student.Address,
        null,
        null,
        null,
        student.LifecycleStatus,
        null,
        0,
        student.AvatarUrl);
}
