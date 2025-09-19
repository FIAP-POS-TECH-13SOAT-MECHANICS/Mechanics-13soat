using Mechanics.Domain.Base;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.WorkOrders;

public class WorkOrder : AbstractEntity
{
    public required Guid CustomerId { get; init; }
    public Customer? Customer { get; init; }

    public required Guid VehicleId { get; init; }
    public Vehicle? Vehicle { get; init; }

    public WorkOrderStatus Status { get; init; }
    public DateTime LastUpdate { get; init; }
    public ICollection<Product>? Products { get; init; }
}
