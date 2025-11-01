using AutoMapper;
using Mechanics.Application.Utils;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Application.Notification.Services;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Mechanics.Application.WorkOrders.Services;

public class WorkOrderAppService(AppDbContext dbContext, IMapper mapper,
    IEmailService emailService, ILogger<WorkOrderAppService> logger) : IAppService
{
    public async Task<Guid> Create(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        if (customer is null) throw new InvalidOperationException("Customer not found.");

        var vehicle = await dbContext.Vehicles.FindAsync(new object[] { request.VehicleId }, cancellationToken);
        if (vehicle is null) throw new InvalidOperationException("Vehicle not found.");

        var existing = await dbContext.WorkOrders.Where(w => w.CustomerId == request.CustomerId).ToListAsync(cancellationToken);
        var accessKey = WorkOrder.GenerateNewAccessKey(existing);

        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            VehicleId = request.VehicleId,
            AccessKey = accessKey,
            Status = WorkOrderStatus.Received,
            LastUpdate = DateTime.UtcNow,
            ReportedProblem = request.ReportedProblem,
        };

        if (request.ProductIds?.Any() == true)
        {
            var products = await dbContext.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync(cancellationToken);
            wo.Products = products;
        }

        if (request.ServiceCatalogIds?.Any() == true)
        {
            var services = await dbContext.ServiceCatalog.Where(s => request.ServiceCatalogIds.Contains(s.Id)).ToListAsync(cancellationToken);
            wo.ServiceCatalog = services;
        }

        await dbContext.WorkOrders.AddAsync(wo, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await emailService.SendWorkOrderCreated(customer, wo, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send WorkOrder created email for {WorkOrderId}", wo.Id);
        }

        return wo.Id;
    }

    public async Task<GetWorkOrderResponse?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var wo = await dbContext.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (wo is null) return null;

        return mapper.Map<GetWorkOrderResponse>(wo);
    }

    public async Task<GetWorkOrderResponse?> TrackByDocumentAndAccessKey(string document, string accessKey, CancellationToken cancellationToken = default)
    {
        var normalizedDocument = new string(document.Where(char.IsDigit).ToArray());
        var normalizedAccessKey = accessKey?.Replace(" ", "") ?? string.Empty;

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document.Number == normalizedDocument, cancellationToken);

        if (customer is null) return null;

        var wo = await dbContext.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);

        if (wo is null) return null;

        return mapper.Map<GetWorkOrderResponse>(wo);
    }

    public async Task RequestApproval(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        var wo = await dbContext.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null) throw new InvalidOperationException("Work order not found.");

        if ((wo.Products == null || !wo.Products.Any()) && (wo.ServiceCatalog == null || !wo.ServiceCatalog.Any()))
            throw new InvalidOperationException("Order must contain at least one product or service to request approval.");

        decimal partsTotal = wo.Products?.Sum(p => GetProductUnitPrice(p)) ?? 0m;
        decimal servicesTotal = wo.ServiceCatalog?.Sum(s => s.BasePrice) ?? 0m;
        var estimatedTotal = partsTotal + servicesTotal;

        var snapshot = new
        {
            Products = wo.Products?.Select(p => new { p.Id, p.Name }),
            Services = wo.ServiceCatalog?.Select(s => new { s.Id, s.Name }),
            EstimatedTotal = estimatedTotal
        };
        wo.ApprovalRequestedAt = DateTime.UtcNow;
        wo.Status = WorkOrderStatus.PendingApproval;
        wo.LastStatusChangeBy = performedByUserId;
        wo.LastUpdate = DateTime.UtcNow;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.UtcNow,
            Action = "RequestApproval",
            Details = $"Estimated total: {estimatedTotal:C}",
            PerformedByUserId = performedByUserId
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customer = await dbContext.Customers.FindAsync(new object[] { wo.CustomerId }, cancellationToken);
        if (customer != null)
        {
            try
            {
                await emailService.SendWorkOrderPendingApproval(customer, wo, estimatedTotal, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send pending approval email for WorkOrder {WorkOrderId}", wo.Id);
            }
        }
    }

    public async Task ChangeStatus(Guid workOrderId, WorkOrderStatus newStatus, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        var wo = await dbContext.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
        if (wo is null) throw new InvalidOperationException("Work order not found.");

        var previous = wo.Status;

        if (previous == WorkOrderStatus.PendingApproval && newStatus == WorkOrderStatus.InProgress && wo.ApprovedAt == null)
            throw new InvalidOperationException("Order must be approved before starting.");

        wo.Status = newStatus;
        wo.LastStatusChangeBy = performedByUserId;
        if (newStatus == WorkOrderStatus.InProgress)
            wo.ApprovedAt ??= DateTime.UtcNow;
        if (newStatus == WorkOrderStatus.Delivered)
            wo.DeliveredAt = DateTime.UtcNow;

        wo.LastUpdate = DateTime.UtcNow;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.UtcNow,
            Action = "StatusChanged",
            Details = $"From {previous} to {newStatus}",
            PerformedByUserId = performedByUserId
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customer = await dbContext.Customers.FindAsync(new object[] { wo.CustomerId }, cancellationToken);
        if (customer != null)
        {
            try
            {
                await emailService.SendWorkOrderStatusChanged(customer, wo, previous.ToString(), newStatus.ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send status changed email for WorkOrder {WorkOrderId}", wo.Id);
            }
        }
    }

    public async Task AddProducts(Guid workOrderId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var wo = await dbContext.WorkOrders
            .Include(w => w.Products)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null) throw new InvalidOperationException("Work order not found.");

        var products = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);
        if (products.Any())
        {
            if (wo.Products is null) wo.Products = new List<Product>();
            foreach (var p in products)
                if (!wo.Products.Any(x => x.Id == p.Id))
                    ((ICollection<Product>)wo.Products).Add(p);

            wo.LastUpdate = DateTime.UtcNow;

            var hist = new WorkOrderHistory
            {
                WorkOrderId = wo.Id,
                OccurredAt = DateTime.UtcNow,
                Action = "ProductsAdded",
                Details = $"Added {products.Count} products",
            };
            await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddServices(Guid workOrderId, IEnumerable<Guid> serviceIds, CancellationToken cancellationToken = default)
    {
        var wo = await dbContext.WorkOrders
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null) throw new InvalidOperationException("Work order not found.");

        var services = await dbContext.ServiceCatalog.Where(s => serviceIds.Contains(s.Id)).ToListAsync(cancellationToken);
        if (services.Any())
        {
            if (wo.ServiceCatalog is null) wo.ServiceCatalog = new List<ServiceCatalog>();
            foreach (var s in services)
                if (!wo.ServiceCatalog.Any(x => x.Id == s.Id))
                    ((ICollection<ServiceCatalog>)wo.ServiceCatalog).Add(s);

            wo.LastUpdate = DateTime.UtcNow;

            var hist = new WorkOrderHistory
            {
                WorkOrderId = wo.Id,
                OccurredAt = DateTime.UtcNow,
                Action = "ServicesAdded",
                Details = $"Added {services.Count} services",
            };
            await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static decimal GetProductUnitPrice(Product p)
    {
        var priceProp = p.GetType().GetProperty("UnitPrice") ?? p.GetType().GetProperty("Price") ?? null;
        if (priceProp != null && priceProp.GetValue(p) is decimal val) return val;
        return 0m;
    }
}
