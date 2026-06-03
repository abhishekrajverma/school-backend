using EduSync.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Infrastructure.Pagination;

public static class QueryPagination
{
    public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
        IQueryable<T> query,
        PaginationQuery pagination,
        CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        return PaginatedList<T>.Create(items, pagination.Page, pagination.PageSize, total);
    }
}
