using Mechanics.Domain.WorkOrders;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using System;
using System.Collections.Generic;

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
            AccessKey = WorkOrder.GenerateNewAccessKey(Array.Empty<WorkOrder>()),
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
            AccessKey = WorkOrder.GenerateNewAccessKey(Array.Empty<WorkOrder>()),
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            ServiceCatalog = new List<ServiceCatalog> { service },
        };
    }

}
