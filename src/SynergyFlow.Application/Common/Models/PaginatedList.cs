using System.Text.Json.Serialization;

namespace SynergyFlow.Application.Common.Models;

[method: JsonConstructor]
public class PaginatedList<T>(IReadOnlyCollection<T> items, int totalCount, int pageNumber, int pageSize)
{
    public int PageNumber { get; init; } = pageNumber;

    public int PageSize { get; init; } = pageSize;

    public int TotalPages { get; init; } =
        pageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

    public int TotalCount { get; init; } = totalCount;

    public IReadOnlyCollection<T> Items { get; init; } = items;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
