namespace Mechanics.Application.WorkOrders.Requests;

/// <summary>
///     Request para atribuir uma WorkOrder a um mecânico.
/// </summary>
public class AssignWorkOrderRequest
{  
    public required Guid AssignedToUserId { get; init; }

    public string? Comment { get; init; }
}
