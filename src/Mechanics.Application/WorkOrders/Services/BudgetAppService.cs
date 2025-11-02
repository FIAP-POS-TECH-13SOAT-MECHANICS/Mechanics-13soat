using Mechanics.Application.Notification.Services;
using Mechanics.Application.Utils;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.WorkOrders.Services;

/// <summary>
///     Serviço para criação, envio e aprovação pública de budgets.
/// </summary>
public class BudgetAppService : IAppService
{
    private readonly AppDbContext dbContext;
    private readonly IEmailService emailService;
    private readonly ILogger<BudgetAppService> logger;

    public BudgetAppService(AppDbContext dbContext, IEmailService emailService, ILogger<BudgetAppService> logger)
    {
        this.dbContext = dbContext;
        this.emailService = emailService;
        this.logger = logger;
    }

    /// <summary>
    ///     Cria um budget a partir dos produtos/serviços atualmente associados à WorkOrder,
    ///     persiste snapshot de preços e itens, define ExpiresAt = CreationDate + 3 dias,
    ///     atualiza WorkOrder.Status para PendingApproval e registra o usuário responsável.
    /// </summary>
    public async Task<Guid> CreateAndSendBudget(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        if (performedByUserId == Guid.Empty)
            throw new BusinessException("PerformedByUserId must be informed.");

        var wo = await dbContext.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        if (wo is null)
            throw new EntityNotFoundException(nameof(WorkOrder), workOrderId.ToString());

        if ((wo.Products == null || !wo.Products.Any()) && (wo.ServiceCatalog == null || !wo.ServiceCatalog.Any()))
            throw new BusinessException("Order must contain at least one product or service to create a budget.");

        var now = DateTime.Now;
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            CreationDate = now,
            ExpiresAt = now.AddDays(3),
            Status = BudgetStatus.Sent,
            Items = new List<BudgetItem>()
        };

        decimal partsTotal = 0m;
        if (wo.Products?.Any() == true)
        {
            var productIds = wo.Products.Select(p => p.Id).ToList();
            var products = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);

            foreach (var p in products)
            {
                var unitPrice = GetProductUnitPrice(p);
                var item = new BudgetItem
                {
                    BudgetId = budget.Id,
                    ProductId = p.Id,
                    NameSnapshot = p.Name,
                    UnitPriceSnapshot = unitPrice,
                    Quantity = 1,
                    Subtotal = unitPrice * 1
                };
                partsTotal += item.Subtotal;
                budget.Items.Add(item);
            }
        }

        decimal servicesTotal = 0m;
        if (wo.ServiceCatalog?.Any() == true)
        {
            var serviceIds = wo.ServiceCatalog.Select(s => s.Id).ToList();
            var services = await dbContext.ServiceCatalog.Where(s => serviceIds.Contains(s.Id)).ToListAsync(cancellationToken);

            foreach (var s in services)
            {
                var item = new BudgetItem
                {
                    BudgetId = budget.Id,
                    ServiceCatalogId = s.Id,
                    NameSnapshot = s.Name,
                    UnitPriceSnapshot = s.BasePrice,
                    Quantity = 1,
                    Subtotal = s.BasePrice * 1
                };
                servicesTotal += item.Subtotal;
                budget.Items.Add(item);
            }
        }

        budget.Total = partsTotal + servicesTotal;

        await dbContext.Budgets.AddAsync(budget, cancellationToken);

        wo.ApprovalRequestedAt = now;
        wo.Status = WorkOrderStatus.PendingApproval;
        wo.LastStatusChangeBy ??= performedByUserId;
        wo.LastUpdate = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = now,
            Action = "BudgetSent",
            Details = $"Budget {budget.Id} sent. Total: {budget.Total:C}",
            PerformedByUserId = performedByUserId
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customer = await dbContext.Customers.FindAsync(new object[] { wo.CustomerId }, cancellationToken);
        if (customer != null)
        {
            try
            {
                await emailService.SendWorkOrderPendingApproval(customer, wo, budget, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send pending approval email for WorkOrder {WorkOrderId}", wo.Id);
            }
        }

        return budget.Id;
    }

    /// <summary>
    ///     Aprova um budget publicamente via documento + accessKey
    /// </summary>
    public async Task PublicApproveBudget(string document, string accessKey, CancellationToken cancellationToken = default)
    {
        var normalizedDocument = new string(document.Where(char.IsDigit).ToArray());
        var normalizedAccessKey = accessKey?.Replace(" ", "") ?? string.Empty;

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document.Number == normalizedDocument, cancellationToken);

        if (customer is null)
            throw new EntityNotFoundException(nameof(Customer), normalizedDocument);

        var wo = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w =>  w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);

        if (wo is null)
            throw new BusinessException("Work order not found or access key invalid.");

        var budget = await dbContext.Budgets
            .Where(b => b.WorkOrderId == wo.Id && b.Status == BudgetStatus.Sent)
            .OrderByDescending(b => b.CreationDate)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);

        if (budget is null)
            throw new BusinessException("No pending budget found for this work order.");

        if (budget.ApprovedAt != null)
            throw new BusinessException("Budget already approved.");

        if (budget.ExpiresAt.HasValue && DateTime.Now > budget.ExpiresAt.Value)
        {
            budget.Status = BudgetStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new BusinessException("Budget expired.");
        }

        // Approve
        budget.Status = BudgetStatus.Approved;
        budget.ApprovedAt = DateTime.Now;
        budget.ApprovedByCustomerDocument = normalizedDocument;

        wo.ApprovedAt ??= DateTime.Now;
        wo.Status = WorkOrderStatus.InProgress;
        wo.LastStatusChangeBy = null;
        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            OccurredAt = DateTime.Now,
            Action = "BudgetApprovedPublic",
            Details = $"Budget {budget.Id} approved by customer {normalizedDocument}.",
            PerformedByUserId = null
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customerEntity = await dbContext.Customers.FindAsync(new object[] { wo.CustomerId }, cancellationToken);
        if (customerEntity != null)
        {
            try
            {
                await emailService.SendWorkOrderStatusChanged(customerEntity, wo, WorkOrderStatus.PendingApproval.ToString(), WorkOrderStatus.InProgress.ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send status changed email after budget approval for WorkOrder {WorkOrderId}", wo.Id);
            }
        }
    }

    private static decimal GetProductUnitPrice(Product p)
    {
        if (p == null) return 0m;

        var unitPriceProp = p.GetType().GetProperty("UnitPrice");
        if (unitPriceProp != null && unitPriceProp.GetValue(p) is decimal up) return up;

        var priceProp = p.GetType().GetProperty("Price");
        if (priceProp != null && priceProp.GetValue(p) is decimal pr) return pr;

        return 0m;
    }
}
