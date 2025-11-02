namespace Mechanics.Application.WorkOrders.Requests;

public class CreateWorkOrderRequest
{
    public required Guid CustomerId { get; init; }
    public required Guid VehicleId { get; init; }
    public string? ReportedProblem { get; init; }
    public IEnumerable<Guid>? ProductIds { get; init; }
    public IEnumerable<Guid>? ServiceCatalogIds { get; init; }
}
