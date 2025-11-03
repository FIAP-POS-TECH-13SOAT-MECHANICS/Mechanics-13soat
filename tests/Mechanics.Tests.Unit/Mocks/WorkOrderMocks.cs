using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

public static class WorkOrderMocks
{
    public static WorkOrder CreateWorkOrderEntity(Guid id, Guid customerId, Guid vehicleId)
    {
        var now = DateTime.Now;
        return new WorkOrder
        {
            Id = id,
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
        };
    }

    public static WorkOrder CreateWorkOrderWithServices(Guid id, Guid customerId, Guid vehicleId, ServiceCatalog service)
    {
        var now = DateTime.Now;
        return new WorkOrder
        {
            Id = id,
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            ServiceCatalog = new List<ServiceCatalog> { service },
        };
    }
}
