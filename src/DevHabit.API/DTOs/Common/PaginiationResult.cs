using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.DTOs.Common;

public sealed record PaginiationResult<T> : ICollectionResponse<T>, ILinkReponse
{
    public required List<T> Items { get; init; }

    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPage => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPage;

    public List<LinkDto> Links { get; set; }

    public static async Task<PaginiationResult<T>> CreateAsync(
        IQueryable<T> query, int page, int pageSize)
    {
        int totalCount = await query.CountAsync();
        List<T> items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        return new PaginiationResult<T>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}