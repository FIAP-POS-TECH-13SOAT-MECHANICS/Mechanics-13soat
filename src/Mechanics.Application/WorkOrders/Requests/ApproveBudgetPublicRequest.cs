namespace Mechanics.Application.WorkOrders.Requests;

/// <summary>
///     Requisição pública para aprovação de budget pelo cliente.
/// </summary>
public class ApproveBudgetPublicRequest
{
    public required string Document { get; init; }
    public required string AccessKey { get; init; }
}
