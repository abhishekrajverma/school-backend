using EduSync.Modules.Students.Application.Dtos;
using EduSync.Modules.Students.Domain;

namespace EduSync.Modules.Students.Application;

public static class StudentMapping
{
    public static StudentDto ToDto(Student student) => new(
        student.ExternalId,
        student.FirstName,
        student.LastName,
        student.FullName,
        student.ClassName,
        student.Section,
        student.RollNo,
        student.AdmissionNo,
        student.Email,
        student.Phone,
        student.DateOfBirth?.ToString("yyyy-MM-dd"),
        student.Gender,
        student.BloodGroup,
        student.Address,
        student.ParentName,
        student.ParentPhone,
        student.ParentEmail,
        student.Status,
        student.FeeStatus,
        student.AttendancePercent,
        student.AvatarUrl);
}
