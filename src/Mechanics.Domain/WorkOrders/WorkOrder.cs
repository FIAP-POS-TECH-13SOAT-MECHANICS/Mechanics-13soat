using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.WorkOrders;

/// <summary>
///     Representa uma ordem de serviço vinculada a um cliente e veículo.
/// </summary>
public partial class WorkOrder : AbstractEntity, IValidatable
{
    private static readonly WorkOrderStatus[] OrderedStatuses = Enum.GetValues<WorkOrderStatus>();

    public required Guid CustomerId { get; init; }
    public Customer? Customer { get; init; }

    /// <summary>
    ///     Chave de acesso para consulta pelo cliente.
    /// </summary>
    public required string AccessKey { get; init; }

    public required Guid VehicleId { get; init; }
    public Vehicle? Vehicle { get; init; }

    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Received;
    public DateTime LastUpdate { get; private set; } = DateTime.UtcNow;

    /// <summary>
    ///     Produtos utilizados na ordem.
    /// </summary>
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    /// <summary>
    ///     Serviços executados na ordem.
    /// </summary>
    public ICollection<ServiceCatalog> ServiceCatalog { get; private set; } = new List<ServiceCatalog>();

    /// <summary>
    ///     Total estimado em serviços.
    /// </summary>
    public decimal ServicesTotal => ServiceCatalog.Sum(service => service.BasePrice);

    /// <summary>
    ///     Quantidade total de produtos associados.
    /// </summary>
    public int ProductsTotal => Products.Count;

    /// <summary>
    ///     Tempo total estimado de execução em minutos.
    /// </summary>
    public int EstimatedExecutionTimeInMinutes => ServiceCatalog.Sum(service => service.AverageTime);

    /// <summary>
    ///     Valor total estimado da ordem.
    /// </summary>
    public decimal TotalAmount => ServicesTotal;

    /// <summary>
    ///     Gera uma nova chave de acesso única por cliente.
    /// </summary>
    /// <param name="existingOrders">As ordens de serviço do cliente.</param>
    /// <remarks>A chave é composta por 8 dígitos e deve ser única por cliente.</remarks>
    /// <returns>Uma nova chave de acesso para ser usada em <see cref="AccessKey"/>.</returns>
    public static string GenerateNewAccessKey(IEnumerable<WorkOrder> existingOrders)
    {
        var existingKeys = existingOrders.Select(order => order.AccessKey).ToHashSet();

        while (true)
        {
            var newKey = string.Concat(Enumerable.Range(0, 8).Select(_ => Random.Shared.Next(0, 10)));
            if (existingKeys.Add(newKey))
                return newKey;
        }
    }

    /// <summary>
    ///     Atualiza o status da ordem de serviço garantindo fluxo válido.
    /// </summary>
    public void UpdateStatus(WorkOrderStatus newStatus)
    {
        if (newStatus == Status)
            return;

        Validator.BuildAndThrow(builder =>
        {
            builder.AddValidation(IsForwardStatus(newStatus), nameof(Status),
                $"Cannot change status from {Status} to {newStatus}.");

            builder.AddConditionalValidation(newStatus == WorkOrderStatus.Delivered, conditional =>
            {
                conditional.AddValidation(Status == WorkOrderStatus.Completed, nameof(Status),
                    "A work order must be completed before it can be delivered.");
            });

            builder.AddConditionalValidation(newStatus == WorkOrderStatus.Completed || newStatus == WorkOrderStatus.Delivered,
                conditional =>
                {
                    conditional.AddValidation(HasItemsAssociated(), nameof(TotalAmount),
                        "A work order needs at least one product or service before it can be finalized.");
                });
        });

        Status = newStatus;
        Touch();
    }

    private static int GetStatusIndex(WorkOrderStatus status) => Array.IndexOf(OrderedStatuses, status);

    private bool IsForwardStatus(WorkOrderStatus newStatus)
    {
        var currentIndex = GetStatusIndex(Status);
        var newIndex = GetStatusIndex(newStatus);

        return newIndex >= currentIndex;
    }

    private bool HasItemsAssociated() => Products.Count > 0 || ServiceCatalog.Count > 0;

    private void EnsureFinalStateIntegrity()
    {
        if (Status is not (WorkOrderStatus.Completed or WorkOrderStatus.Delivered))
            return;

        Validator.BuildAndThrow(builder =>
        {
            builder.AddValidation(HasItemsAssociated(), nameof(TotalAmount),
                "Completed work orders must contain at least one product or service.");
        });
    }

    private void Touch() => LastUpdate = DateTime.UtcNow;
}
