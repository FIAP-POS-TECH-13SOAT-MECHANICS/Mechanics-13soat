namespace Mechanics.Application.WorkOrders.Requests;

public class UpdateWorkOrderRequest
{
    public IEnumerable<Guid>? ProductIds { get; init; }
    public IEnumerable<Guid>? ServiceIds { get; init; }
    public string? Observations { get; init; }
}
