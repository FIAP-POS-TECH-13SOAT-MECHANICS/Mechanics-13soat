using Mechanics.Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Utils.PagedList;

public static class PaginatedListQueryExtension
{
    public static async Task<(IEnumerable<T> Items, int TotalCount)> GetPaginatedList<T, TRequest>(this IQueryable<T> queryable,
        TRequest request, CancellationToken cancellationToken = default)
        where T : AbstractEntity where TRequest : PaginatedListRequest
    {
        var count = await queryable.CountAsync(cancellationToken);
        var items = count > 0
            ? await queryable
                .OrderBy(t => t.Id)
                .Skip(request.ItemsPerPage * (request.Page - 1))
                .Take(request.Page * request.ItemsPerPage)
                .ToListAsync(cancellationToken)
            : [];

        return (items, count);
    }
}
