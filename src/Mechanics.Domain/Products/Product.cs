using Mechanics.Domain.Base;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Domain.Products;

public class Product : AbstractEntity
{
    public required string Description { get; init; }
    public required ProductType Type { get; init; }
    public IEnumerable<WorkOrder>? WorkOrders { get; init; }
}
