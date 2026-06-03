namespace EduSync.Modules.Identity.Domain;

public static class UserRoles
{
    public const string Admin = "admin";
    public const string Principal = "principal";
    public const string Teacher = "teacher";
    public const string Student = "student";
    public const string Parent = "parent";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Admin, Principal, Teacher, Student, Parent,
    };
}
