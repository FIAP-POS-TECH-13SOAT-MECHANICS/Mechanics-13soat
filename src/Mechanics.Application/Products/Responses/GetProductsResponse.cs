using Mechanics.Application.Utils.PagedList;
using Mechanics.Domain.Products;

namespace Mechanics.Application.Products.Responses;

public class GetProductsResponse(IEnumerable<GetProductResponse> items, int totalCount)
    : PaginatedListResponse<GetProductResponse>(items, totalCount);
