using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders.Requests;

public class ChangeStatusRequest
{
    public required WorkOrderStatus NewStatus { get; init; }
    public string? Comment { get; init; }
}
