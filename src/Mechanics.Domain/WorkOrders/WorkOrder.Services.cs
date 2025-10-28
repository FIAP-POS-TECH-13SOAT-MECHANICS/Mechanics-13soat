using Mechanics.Domain.ServicesCatalog;

namespace Mechanics.Domain.WorkOrders;

public partial class WorkOrder
{
    /// <summary>
    ///     Substitui os serviços associados à ordem mantendo as garantias do agregado.
    /// </summary>
    public void SetServices(IEnumerable<ServiceCatalog>? services)
    {
        var serviceList = (services ?? Enumerable.Empty<ServiceCatalog>()).DistinctBy(service => service.Id).ToList();

        ServiceCatalog = serviceList;
        EnsureFinalStateIntegrity();
        Touch();
    }

    /// <summary>
    ///     Adiciona um serviço ao orçamento.
    /// </summary>
    public void AddService(ServiceCatalog service)
    {
        ArgumentNullException.ThrowIfNull(service);

        var updatedServices = ServiceCatalog.Append(service);
        SetServices(updatedServices);
    }

    /// <summary>
    ///     Remove um serviço associado.
    /// </summary>
    public void RemoveService(Guid serviceId)
    {
        var updatedServices = ServiceCatalog.Where(service => service.Id != serviceId).ToList();

        if (updatedServices.Count == ServiceCatalog.Count)
            return;

        SetServices(updatedServices);
    }
}
