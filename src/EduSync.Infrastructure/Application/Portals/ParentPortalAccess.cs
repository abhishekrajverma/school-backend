using EduSync.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Application.Portals;

internal static class ParentPortalAccess
{
    public static async Task<bool> IsLinkedChildAsync(
        EduSyncDbContext db,
        Guid userId,
        string childExternalId,
        CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return false;
        }

        var parent = await db.Parents.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == user.Email && !p.IsDeleted, ct);
        if (parent is null)
        {
            return false;
        }

        var student = await db.Students.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == childExternalId && !s.IsDeleted, ct);
        if (student is null)
        {
            return false;
        }

        return await db.StudentParents.AsNoTracking()
            .AnyAsync(
                sp => sp.ParentId == parent.Id
                      && sp.StudentId == student.Id
                      && sp.IsActive
                      && !sp.IsDeleted,
                ct);
    }
}
