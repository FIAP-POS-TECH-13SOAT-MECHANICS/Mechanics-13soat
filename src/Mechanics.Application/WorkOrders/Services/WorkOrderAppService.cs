using AutoMapper;
using Mechanics.Application.Notification.Services;
using Mechanics.Application.Utils;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.WorkOrders.Services;

public class WorkOrderAppService(
    AppDbContext db,
    IMapper mapper,
    IEmailService emailService,
    ILogger<WorkOrderAppService> logger,
    BudgetAppService budgetService)
    : IAppService
{
    /// <summary>
    ///     Cria uma nova WorkOrder.
    /// </summary>
    public async Task<Guid> Create(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
    {
        var vehicle = await db.Vehicles
           .Include(v => v.Owner)
           .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);

        if (vehicle is null)
            throw new EntityNotFoundException(nameof(Vehicle), request.VehicleId.ToString());


        var customer = vehicle.Owner ?? await db.Customers.FindAsync(vehicle.OwnerId, cancellationToken);
        if (customer is null)
            throw new EntityNotFoundException(nameof(Customer), vehicle.OwnerId.ToString());

        var existing = await db.WorkOrders.Where(w => w.CustomerId == customer.Id).ToListAsync(cancellationToken);

        var accessKey = WorkOrder.GenerateNewAccessKey(existing);

        var now = DateTime.Now;

        var wo = new WorkOrder
        {
            CustomerId = customer.Id,
            VehicleId = request.VehicleId,
            AccessKey = accessKey,
            Status = WorkOrderStatus.Received,
            CreationDate = now,
            LastUpdate = now,
            ReportedProblem = request.ReportedProblem,
        };

        if (request.ProductIds?.Any() == true)
        {
            var products = await db.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync(cancellationToken);
            wo.Products = products;
        }

        if (request.ServiceCatalogIds?.Any() == true)
        {
            var services = await db.ServiceCatalog.Where(s => request.ServiceCatalogIds.Contains(s.Id))
                .ToListAsync(cancellationToken);
            wo.ServiceCatalog = services;
        }

        await db.WorkOrders.AddAsync(wo, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

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

    /// <summary>
    ///     Obtém detalhes de uma WorkOrder por id.
    /// </summary>
    public async Task<GetWorkOrderResponse?> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        return wo is null ? null : mapper.Map<GetWorkOrderResponse>(wo);
    }

    /// <summary>
    ///     Consulta pública por documento do cliente e accessKey.
    ///     Usado pelo cliente para acompanhar o progresso da OS.
    /// </summary>
    public async Task<GetWorkOrderResponse?> TrackByDocumentAndAccessKey(string document, string accessKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedDocument = new string(document.Where(char.IsDigit).ToArray());
        var normalizedAccessKey = accessKey.Replace(" ", "");

        var customer = await db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document.Number == normalizedDocument, cancellationToken);

        if (customer is null) return null;

        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);

        return wo is null ? null : mapper.Map<GetWorkOrderResponse>(wo);
    }

    /// <summary>
    ///     Solicita aprovação do orçamento para a ordem.
    /// </summary>
    public async Task RequestApproval(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        var woExists = await db.WorkOrders.AnyAsync(w => w.Id == workOrderId, cancellationToken);
        if (!woExists)
            throw new EntityNotFoundException(nameof(WorkOrder), workOrderId.ToString());

        await budgetService.CreateAndSendBudget(workOrderId, performedByUserId, cancellationToken);
    }

    /// <summary>
    ///     Altera o status da WorkOrder seguindo regras do fluxo principal.
    ///     Received -> UnderDiagnosis -> PendingApproval -> InProgress -> Completed -> Delivered
    /// </summary>
    public async Task ChangeStatus(Guid workOrderId, WorkOrderStatus newStatus, Guid performedByUserId,
        CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
        if (wo is null)
            throw new EntityNotFoundException(nameof(WorkOrder), workOrderId.ToString());

        var previous = wo.Status;

        if (previous == newStatus)
            throw new BusinessException($"Work order is already in {newStatus}.");

        if (!IsTransitionAllowed(previous, newStatus))
            throw new BusinessException($"Invalid status transition from {previous} to {newStatus}.");       

        if (newStatus == WorkOrderStatus.InProgress)
        {
            var approved = wo.ApprovedAt != null ||
                           await db.Budgets.AnyAsync(b => b.WorkOrderId == workOrderId && b.Status == BudgetStatus.Approved,
                               cancellationToken);

            if (!approved)
                throw new BusinessException("Order must be approved before starting.");
        }

        wo.Status = newStatus;
        wo.LastStatusChangeBy = performedByUserId;
        if (newStatus == WorkOrderStatus.InProgress)
            wo.ApprovedAt ??= DateTime.Now;
        if (newStatus == WorkOrderStatus.Delivered)
            wo.DeliveredAt = DateTime.Now;

        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.Now,
            Action = "StatusChanged",
            Details = $"From {previous} to {newStatus}",
            PerformedByUserId = performedByUserId,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        // notifica cliente sobre a mudança de status
        var customer = await db.Customers.FindAsync(wo.CustomerId, cancellationToken);
        if (customer != null)
        {
            try
            {
                await emailService.SendWorkOrderStatusChanged(customer, wo, previous, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send status changed email for WorkOrder {WorkOrderId}", wo.Id);
            }
        }
    }

    /// <summary>
    ///     Adiciona produtos (peças/insumos) à ordem. 
    /// </summary>
    public async Task AddProducts(Guid workOrderId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null)
            throw new EntityNotFoundException(nameof(WorkOrder), workOrderId.ToString());

        var addedProducts = await ApplyProductsToWorkOrderAsync(wo, productIds, cancellationToken);
        if (addedProducts == 0) return;

        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.Now,
            Action = "ProductsAdded",
            Details = $"Added {addedProducts} products",
            PerformedByUserId = null,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Adiciona serviços à ordem.
    /// </summary>
    public async Task AddServices(Guid workOrderId, IEnumerable<Guid> serviceIds, CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null)
            throw new EntityNotFoundException(nameof(WorkOrder), workOrderId.ToString());

        var addedServices = await ApplyServicesToWorkOrderAsync(wo, serviceIds, cancellationToken);
        if (addedServices == 0) return;

        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.Now,
            Action = "ServicesAdded",
            Details = $"Added {addedServices} services",
            PerformedByUserId = null,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Atualiza produtos, serviços e observações da ordem.
    /// </summary>
    public async Task UpdateDetails(Guid workOrderId, UpdateWorkOrderRequest request, Guid performedByUserId,
        CancellationToken cancellationToken = default)
    {
        var wo = await db.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null)
            throw new EntityNotFoundException(nameof(WorkOrder), workOrderId.ToString());

        var addedProducts = await ApplyProductsToWorkOrderAsync(wo, request.ProductIds, cancellationToken);
        var addedServices = await ApplyServicesToWorkOrderAsync(wo, request.ServiceIds, cancellationToken);

        var observationChanged = false;
        if (request.Observations is not null)
        {
            if (wo.Observations != request.Observations)
            {
                wo.Observations = request.Observations;
                observationChanged = true;
            }
        }

        if (addedProducts == 0 && addedServices == 0 && !observationChanged)
            return;

        wo.LastUpdate = DateTime.Now;

        var detailsParts = new List<string>();
        if (addedProducts > 0) detailsParts.Add($"AddedProducts:{addedProducts}");
        if (addedServices > 0) detailsParts.Add($"AddedServices:{addedServices}");
        if (observationChanged) detailsParts.Add("ObservationsUpdated");

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.Now,
            Action = "DetailsUpdated",
            Details = string.Join("; ", detailsParts),
            PerformedByUserId = performedByUserId,
        };
        await db.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    ///     Helper: aplica produtos retorna quantidade adicionada.
    /// </summary>
    private async Task<int> ApplyProductsToWorkOrderAsync(WorkOrder wo, IEnumerable<Guid>? productIds, CancellationToken cancellationToken)
    {
        if (productIds is null) return 0;
        var ids = productIds as IList<Guid> ?? productIds.ToArray();
        if (!ids.Any()) return 0;

        var products = await db.Products.Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);
        if (products.Count == 0) return 0;

        wo.Products ??= new List<Product>();

        var added = 0;
        foreach (var p in products.Where(p => wo.Products.All(x => x.Id != p.Id)))
        {
            wo.Products.Add(p);
            added++;
        }

        return added;
    }

    /// <summary>
    ///     Helper: aplica serviços retorna quantidade adicionada.
    /// </summary>
    private async Task<int> ApplyServicesToWorkOrderAsync(WorkOrder wo, IEnumerable<Guid>? serviceIds, CancellationToken cancellationToken)
    {
        if (serviceIds is null) return 0;
        var ids = serviceIds as IList<Guid> ?? serviceIds.ToArray();
        if (!ids.Any()) return 0;

        var services = await db.ServiceCatalog.Where(s => ids.Contains(s.Id)).ToListAsync(cancellationToken);
        if (services.Count == 0) return 0;

        wo.ServiceCatalog ??= new List<ServiceCatalog>();

        var added = 0;
        foreach (var s in services.Where(s => wo.ServiceCatalog.All(x => x.Id != s.Id)))
        {
            wo.ServiceCatalog.Add(s);
            added++;
        }

        return added;
    }

    private static bool IsTransitionAllowed(WorkOrderStatus from, WorkOrderStatus to)
    {
        return (from, to) switch
        {
            (WorkOrderStatus.Received, WorkOrderStatus.UnderDiagnosis) => true,
            (WorkOrderStatus.UnderDiagnosis, WorkOrderStatus.PendingApproval) => true,
            (WorkOrderStatus.PendingApproval, WorkOrderStatus.InProgress) => true,
            (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
            (WorkOrderStatus.Completed, WorkOrderStatus.Delivered) => true,
            _ => false,
        };
    }
}
