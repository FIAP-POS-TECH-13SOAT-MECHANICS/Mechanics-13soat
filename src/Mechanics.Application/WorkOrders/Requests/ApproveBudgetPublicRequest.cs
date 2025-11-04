namespace Mechanics.Application.WorkOrders.Requests;

/// <summary>
///     Requisição pública para aprovação de budget pelo cliente.
/// </summary>
public class ApproveBudgetPublicRequest
{
    public required string Document { get; init; }
    public required string AccessKey { get; init; }

    /// <summary>
    /// Comentário opcional informado pelo cliente (para aprovar ou rejeitar).
    /// </summary>
    public string? Description { get; init; }
}
