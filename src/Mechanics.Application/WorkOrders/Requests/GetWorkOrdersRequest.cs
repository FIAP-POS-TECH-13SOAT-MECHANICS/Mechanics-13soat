using Mechanics.Application.Utils.PagedList;

namespace Mechanics.Application.WorkOrders.Requests;

public class GetWorkOrdersRequest : PaginatedListRequest
{
    /// <summary>
    ///     Filtro por ID do cliente (Customer).
    /// </summary>
    public Guid? CustomerId { get; init; }

    /// <summary>
    ///     Filtro por ID do veículo.
    /// </summary>
    public Guid? VehicleId { get; init; }
}
