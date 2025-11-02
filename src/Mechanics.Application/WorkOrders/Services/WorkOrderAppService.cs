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

namespace Mechanics.Application.WorkOrders.Services
{
    public class WorkOrderAppService : IAppService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly ILogger<WorkOrderAppService> _logger;
        private readonly BudgetAppService _budgetService;

        public WorkOrderAppService(
            AppDbContext db,
            IMapper mapper,
            IEmailService emailService,
            ILogger<WorkOrderAppService> logger,
            BudgetAppService budgetService)
        {
            _db = db;
            _mapper = mapper;
            _emailService = emailService;
            _logger = logger;
            _budgetService = budgetService;
        }

        /// <summary>
        ///     Cria uma nova WorkOrder.
        /// </summary>
        public async Task<Guid> Create(CreateWorkOrderRequest request, CancellationToken cancellationToken = default)
        {
            var customer = await _db.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
            if (customer is null) throw new InvalidOperationException("Customer not found.");

            var vehicle = await _db.Vehicles.FindAsync(new object[] { request.VehicleId }, cancellationToken);
            if (vehicle is null) throw new InvalidOperationException("Vehicle not found.");

            var existing = await _db.WorkOrders.Where(w => w.CustomerId == request.CustomerId).ToListAsync(cancellationToken);
            var accessKey = WorkOrder.GenerateNewAccessKey(existing);

            var Now = DateTime.Now;

            var wo = new WorkOrder
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                AccessKey = accessKey,
                Status = WorkOrderStatus.Received,
                CreatedAt = Now,
                LastUpdate = Now,
                ReportedProblem = request.ReportedProblem,
            };

            if (request.ProductIds?.Any() == true)
            {
                var products = await _db.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync(cancellationToken);
                wo.Products = products;
            }

            if (request.ServiceCatalogIds?.Any() == true)
            {
                var services = await _db.ServiceCatalog.Where(s => request.ServiceCatalogIds.Contains(s.Id)).ToListAsync(cancellationToken);
                wo.ServiceCatalog = services;
            }

            await _db.WorkOrders.AddAsync(wo, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            try
            {
                await _emailService.SendWorkOrderCreated(customer, wo, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send WorkOrder created email for {WorkOrderId}", wo.Id);
            }

            return wo.Id;
        }

        /// <summary>
        ///     Obtém detalhes de uma WorkOrder por id.
        /// </summary>
        public async Task<GetWorkOrderResponse?> Get(Guid id, CancellationToken cancellationToken = default)
        {
            var wo = await _db.WorkOrders
                .Include(w => w.Products)
                .Include(w => w.ServiceCatalog)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

            if (wo is null) return null;

            return _mapper.Map<GetWorkOrderResponse>(wo);
        }

        /// <summary>
        ///     Consulta pública por documento do cliente e accessKey.
        ///     Usado pelo cliente para acompanhar o progresso da OS.
        /// </summary>
        public async Task<GetWorkOrderResponse?> TrackByDocumentAndAccessKey(string document, string accessKey, CancellationToken cancellationToken = default)
        {
            var normalizedDocument = new string((document ?? string.Empty).Where(char.IsDigit).ToArray());
            var normalizedAccessKey = (accessKey ?? string.Empty).Replace(" ", "");

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Document.Number == normalizedDocument, cancellationToken);

            if (customer is null) return null;

            var wo = await _db.WorkOrders
                .Include(w => w.Products)
                .Include(w => w.ServiceCatalog)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);

            if (wo is null) return null;

            return _mapper.Map<GetWorkOrderResponse>(wo);
        }

        /// <summary>
        ///     Solicita aprovação do orçamento para a ordem.
        /// </summary>
        public async Task RequestApproval(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
        {
            // valida existência da ordem antes de delegar
            var woExists = await _db.WorkOrders.AnyAsync(w => w.Id == workOrderId, cancellationToken);
            if (!woExists) throw new InvalidOperationException("Work order not found.");

            await _budgetService.CreateAndSendBudget(workOrderId, cancellationToken);
        }

        /// <summary>
        ///     Altera o status da WorkOrder seguindo regras do fluxo principal.
        ///     Received -> UnderDiagnosis -> PendingApproval -> InProgress -> Completed -> Delivered
        /// </summary>
        public async Task ChangeStatus(Guid workOrderId, WorkOrderStatus newStatus, Guid performedByUserId, CancellationToken cancellationToken = default)
        {
            var wo = await _db.WorkOrders.FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);
            if (wo is null) throw new InvalidOperationException("Work order not found.");

            var previous = wo.Status;

            if (!IsTransitionAllowed(previous, newStatus))
                throw new InvalidOperationException($"Invalid status transition from {previous} to {newStatus}.");

            if (newStatus == WorkOrderStatus.InProgress)
            {
                var approved = wo.ApprovedAt != null
                    || await _db.Budgets.AnyAsync(b => b.WorkOrderId == workOrderId && b.Status == BudgetStatus.Approved, cancellationToken);

                if (!approved)
                    throw new InvalidOperationException("Order must be approved before starting.");
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
                PerformedByUserId = performedByUserId
            };
            await _db.WorkOrderHistories.AddAsync(hist, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);

            // notifica cliente sobre a mudança de status
            var customer = await _db.Customers.FindAsync(new object[] { wo.CustomerId }, cancellationToken);
            if (customer != null)
            {
                try
                {
                    await _emailService.SendWorkOrderStatusChanged(customer, wo, previous.ToString(), newStatus.ToString(), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send status changed email for WorkOrder {WorkOrderId}", wo.Id);
                }
            }
        }

        /// <summary>
        ///     Adiciona produtos (peças/insumos) à ordem.
        /// </summary>
        public async Task AddProducts(Guid workOrderId, IEnumerable<Guid> productIds, CancellationToken cancellationToken = default)
        {
            var wo = await _db.WorkOrders
                .Include(w => w.Products)
                .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

            if (wo is null) throw new InvalidOperationException("Work order not found.");

            var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);
            if (!products.Any()) return;

            if (wo.Products is null) wo.Products = new List<Product>();

            foreach (var p in products)
            {
                if (!wo.Products.Any(x => x.Id == p.Id))
                    ((ICollection<Product>)wo.Products).Add(p);
            }

            wo.LastUpdate = DateTime.Now;

            var hist = new WorkOrderHistory
            {
                WorkOrderId = wo.Id,
                OccurredAt = DateTime.Now,
                Action = "ProductsAdded",
                Details = $"Added {products.Count} products",
                PerformedByUserId = null
            };
            await _db.WorkOrderHistories.AddAsync(hist, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///     Adiciona serviços à ordem.
        /// </summary>
        public async Task AddServices(Guid workOrderId, IEnumerable<Guid> serviceIds, CancellationToken cancellationToken = default)
        {
            var wo = await _db.WorkOrders
                .Include(w => w.ServiceCatalog)
                .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

            if (wo is null) throw new InvalidOperationException("Work order not found.");

            var services = await _db.ServiceCatalog.Where(s => serviceIds.Contains(s.Id)).ToListAsync(cancellationToken);
            if (!services.Any()) return;

            if (wo.ServiceCatalog is null) wo.ServiceCatalog = new List<ServiceCatalog>();

            foreach (var s in services)
            {
                if (!wo.ServiceCatalog.Any(x => x.Id == s.Id))
                    ((ICollection<ServiceCatalog>)wo.ServiceCatalog).Add(s);
            }

            wo.LastUpdate = DateTime.Now;

            var hist = new WorkOrderHistory
            {
                WorkOrderId = wo.Id,
                OccurredAt = DateTime.Now,
                Action = "ServicesAdded",
                Details = $"Added {services.Count} services",
                PerformedByUserId = null
            };
            await _db.WorkOrderHistories.AddAsync(hist, cancellationToken);

            await _db.SaveChangesAsync(cancellationToken);
        }
        
        private static bool IsTransitionAllowed(WorkOrderStatus from, WorkOrderStatus to)
        {
            if (from == to) return true;

            return (from, to) switch
            {
                (WorkOrderStatus.Received, WorkOrderStatus.UnderDiagnosis) => true,
                (WorkOrderStatus.UnderDiagnosis, WorkOrderStatus.PendingApproval) => true,
                (WorkOrderStatus.PendingApproval, WorkOrderStatus.InProgress) => true,
                (WorkOrderStatus.InProgress, WorkOrderStatus.Completed) => true,
                (WorkOrderStatus.Completed, WorkOrderStatus.Delivered) => true,
                _ => false
            };
        }
    }
}
