using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.WorkOrders.Responses;

public class GetBudgetResponse
{
    public required Guid Id { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public required BudgetStatus Status { get; init; }
    public decimal Total { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public string? ApprovedByCustomerDocument { get; init; }
    public IEnumerable<GetBudgetItemResponse>? Items { get; init; }
}

public class GetBudgetItemResponse
{
    public Guid? ProductId { get; init; }
    public Guid? ServiceCatalogId { get; init; }
    public string NameSnapshot { get; init; } = default!;
    public decimal UnitPriceSnapshot { get; init; }
    public int Quantity { get; init; }
    public decimal Subtotal { get; init; }
}
