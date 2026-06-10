using EduSync.Modules.Parents.Application.Dtos;
using EduSync.Modules.Parents.Domain;

namespace EduSync.Modules.Parents.Application;

public static class ParentMapping
{
    public static ParentDto ToDto(
        Parent p,
        IReadOnlyList<string> children,
        IReadOnlyList<string> studentIds) => new(
        p.ExternalId,
        p.FirstName,
        p.LastName,
        p.FullName,
        p.Email,
        p.Phone,
        p.Occupation,
        p.Address,
        children,
        studentIds,
        p.LifecycleStatus,
        p.AvatarUrl);
}
