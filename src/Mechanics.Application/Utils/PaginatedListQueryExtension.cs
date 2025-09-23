using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Utils;

public static class PaginatedListQueryExtension
{
    public static async Task<(IEnumerable<T> Items, int TotalCount)> GetPaginatedList<T, TRequest>(this IQueryable<T> queryable,
        TRequest request, CancellationToken cancellationToken = default)
        where TRequest : PaginatedListRequest
    {
        var count = await queryable.CountAsync(cancellationToken);
        var items = count > 0
            ? await queryable
                .Skip(request.ItemsPerPage * (request.Page - 1))
                .Take(request.Page * request.ItemsPerPage)
                .ToListAsync(cancellationToken)
            : [];

        return (items, count);
    }
}
