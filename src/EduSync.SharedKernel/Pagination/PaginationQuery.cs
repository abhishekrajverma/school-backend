namespace EduSync.SharedKernel.Pagination;

public sealed class PaginationQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public string SortOrder { get; init; } = "asc";

    public bool IsDescending => string.Equals(SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

    public static PaginationQuery FromHttp(int? page, int? pageSize, string? search, string? sortBy, string? sortOrder)
    {
        var normalizedPage = page is null or < 1 ? 1 : page.Value;
        var normalizedSize = pageSize is null or < 1 ? 20 : Math.Min(pageSize.Value, 100);

        return new PaginationQuery
        {
            Page = normalizedPage,
            PageSize = normalizedSize,
            Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            SortBy = string.IsNullOrWhiteSpace(sortBy) ? null : sortBy.Trim(),
            SortOrder = string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder.Trim(),
        };
    }
}
