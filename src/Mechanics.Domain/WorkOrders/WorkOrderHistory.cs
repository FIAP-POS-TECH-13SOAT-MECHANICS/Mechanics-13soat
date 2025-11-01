using Mechanics.Domain.Base;

namespace Mechanics.Domain.WorkOrders;

public class WorkOrderHistory : AbstractEntity
{
    public required Guid WorkOrderId { get; set; }
    public WorkOrder? WorkOrder { get; set; }

    public required DateTime OccurredAt { get; set; }
    public required string Action { get; set; } = default!; 
    public string? Details { get; set; }
    public Guid? PerformedByUserId { get; set; }
}
