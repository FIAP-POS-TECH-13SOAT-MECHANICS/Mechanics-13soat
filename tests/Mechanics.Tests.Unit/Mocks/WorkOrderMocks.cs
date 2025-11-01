using System;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Tests.Unit.Mocks;

public static class WorkOrderMocks
{
    public static CreateWorkOrderRequest BuildCreateRequest(Guid customerId, Guid vehicleId, string reportedProblem = "Ruído no motor")
        => new()
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            ReportedProblem = reportedProblem,
        };

    public static WorkOrder CreateWorkOrderEntity(Guid id, Guid customerId, Guid vehicleId, ServiceCatalog[]? services = null, Product[]? products = null)
        => new()
        {
            Id = id,
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = "00000001",
            Status = WorkOrderStatus.Received,
            CreationDate = DateTime.UtcNow,
            LastUpdate = DateTime.UtcNow,
            ServiceCatalog = services,
            Products = products,
        };

    public static WorkOrder CreateWorkOrderWithServices(Guid id, Guid customerId, Guid vehicleId, params ServiceCatalog[] services)
        => CreateWorkOrderEntity(id, customerId, vehicleId, services, null);

    public static WorkOrder CreateWorkOrderWithProducts(Guid id, Guid customerId, Guid vehicleId, params Product[] products)
        => CreateWorkOrderEntity(id, customerId, vehicleId, null, products);
}
