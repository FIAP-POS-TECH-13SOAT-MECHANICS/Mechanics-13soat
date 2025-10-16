using Mechanics.Domain.Base;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.Vehicles;

namespace Mechanics.Domain.WorkOrders;

public class WorkOrder : AbstractEntity
{
    public required Guid CustomerId { get; init; }
    public Customer? Customer { get; init; }

    /// <summary>
    ///     Chave de acesso para consulta pelo cliente.
    /// </summary>
    public required string AccessKey { get; init; }

    public required Guid VehicleId { get; init; }
    public Vehicle? Vehicle { get; init; }
    public WorkOrderStatus Status { get; init; }
    public DateTime LastUpdate { get; init; }
    public ICollection<Product>? Products { get; init; }

    /// <summary>
    ///     Gera uma nova chave de acesso.
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
}
