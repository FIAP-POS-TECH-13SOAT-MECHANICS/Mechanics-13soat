using Mechanics.Application.Notification.Services;
using Mechanics.Application.Utils;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.WorkOrders.Services;

/// <summary>
///     Serviço para criação, envio e aprovação pública de budgets.
/// </summary>
public class BudgetAppService(AppDbContext dbContext, IEmailService emailService, ILogger<BudgetAppService> logger)
    : IAppService
{
    /// <summary>
    ///     Cria um <see cref="Budget"/> a partir dos produtos/serviços atualmente associados à WorkOrder,
    ///     persiste snapshot de preços e itens, define ExpiresAt = CreationDate + 3 dias,
    ///     atualiza <see cref="WorkOrder.Status"/> para PendingApproval e registra o usuário responsável.
    /// </summary>
    public async Task CreateAndSendBudget(Guid workOrderId, Guid performedByUserId, CancellationToken cancellationToken = default)
    {
        if (performedByUserId == Guid.Empty)
            throw new BusinessException("PerformedByUserId must be informed.");

        var wo = await dbContext.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == workOrderId, cancellationToken);

        EntityNotFoundException.ThrowIfNull(wo, workOrderId);

        if ((wo.Products == null || wo.Products.Count == 0) && (wo.ServiceCatalog == null || wo.ServiceCatalog.Count == 0))
            throw new BusinessException("Order must contain at least one product or service to create a budget.");

        var now = DateTime.Now;
        var budget = new Budget
        {
            WorkOrderId = wo.Id,
            CreationDate = now,
            ExpiresAt = now.AddDays(3),
            Status = BudgetStatus.Sent,
            Items = new List<BudgetItem>(),
        };

        var partsTotal = 0m;
        if (wo.Products?.Count > 0)
        {
            var productIds = wo.Products.Select(p => p.Id).ToList();
            var products = await dbContext.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(cancellationToken);

            foreach (var item in products.Select(p => new BudgetItem
                     {
                         BudgetId = budget.Id,
                         ProductId = p.Id,
                         NameSnapshot = p.Name,
                         UnitPriceSnapshot = p.UnitPrice,
                         Quantity = 1,
                         Subtotal = p.UnitPrice * 1,
                     }))
            {
                partsTotal += item.Subtotal;
                budget.Items.Add(item);
            }
        }

        var servicesTotal = 0m;
        if (wo.ServiceCatalog?.Count > 0)
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
                    Subtotal = s.BasePrice * 1,
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
            Action = "BudgetSent",
            Details = $"Budget {budget.Id} sent. Total: {budget.Total:C}",
            PerformedByUserId = performedByUserId,
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customer = await dbContext.Customers.FindAsync([wo.CustomerId], cancellationToken);
        if (customer == null)
            return;

        try
        {
            await emailService.SendWorkOrderPendingApproval(customer, wo, budget, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send pending approval email for WorkOrder {WorkOrderId}", wo.Id);
        }
    }

    /// <summary>
    ///     Aprova um budget publicamente via documento + accessKey
    /// </summary>
    public async Task PublicApproveBudget(string document, string accessKey, string? description = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedDocument = new string(document.Where(char.IsDigit).ToArray());
        var normalizedAccessKey = accessKey.Replace(" ", "");

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document.Number == normalizedDocument, cancellationToken);
        EntityNotFoundException.ThrowIfNull(customer, document);

        var wo = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, accessKey);

        var budget = await dbContext.Budgets
            .Where(b => b.WorkOrderId == wo.Id && b.Status == BudgetStatus.Sent)
            .OrderByDescending(b => b.CreationDate)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);
        EntityNotFoundException.ThrowIfNull(budget, budget?.WorkOrderId);

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
        budget.Description = description;
        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "BudgetApprovedPublic",
            Details = description is null
                ? $"Budget {budget.Id} approved by customer {normalizedDocument}."
                : $"Budget {budget.Id} approved by customer {normalizedDocument}. Description: {description}",
            PerformedByUserId = null,
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await emailService.SendWorkOrderStatusChanged(customer, wo, WorkOrderStatus.PendingApproval, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send status changed email after budget approval for WorkOrder {WorkOrderId}",
                wo.Id);
        }

        if (wo.AssignedToUserId != null)
        {
            var mechanic = await dbContext.Users.FindAsync([wo.AssignedToUserId], cancellationToken);
            EntityNotFoundException.ThrowIfNull(mechanic, wo.AssignedToUserId);

            try
            {
                await emailService.SendMechanicBudgetDecision(mechanic, wo, budget, approved: true, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send mechanic notification for approved budget {BudgetId}", budget.Id);
            }
        }
    }

    /// <summary>
    ///     Rejeita um budget publicamente via documento + accessKey.
    ///     Marca o budget como Rejected, coloca a OS novamente em UnderDiagnosis e notifica o mecânico.
    /// </summary>
    public async Task PublicRejectBudget(string document, string accessKey, string? description = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedDocument = new string(document.Where(char.IsDigit).ToArray());
        var normalizedAccessKey = accessKey.Replace(" ", "");

        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Document.Number == normalizedDocument, cancellationToken);
        EntityNotFoundException.ThrowIfNull(customer, document);

        var wo = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w => w.CustomerId == customer.Id && w.AccessKey == normalizedAccessKey, cancellationToken);
        EntityNotFoundException.ThrowIfNull(wo, accessKey);

        var budget = await dbContext.Budgets
            .Where(b => b.WorkOrderId == wo.Id && b.Status == BudgetStatus.Sent)
            .OrderByDescending(b => b.CreationDate)
            .Include(b => b.Items)
            .FirstOrDefaultAsync(cancellationToken);
        EntityNotFoundException.ThrowIfNull(budget, wo.Id);

        if (budget.ExpiresAt.HasValue && DateTime.Now > budget.ExpiresAt.Value)
        {
            budget.Status = BudgetStatus.Expired;
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new BusinessException("Budget expired.");
        }

        budget.Status = BudgetStatus.Rejected;
        budget.RejectedAt = DateTime.Now;
        budget.Description = description;

        wo.Status = WorkOrderStatus.UnderDiagnosis;
        wo.LastUpdate = DateTime.Now;

        var hist = new WorkOrderHistory
        {
            WorkOrderId = wo.Id,
            Action = "BudgetRejectedByCustomer",
            Details = description is null
                ? $"Budget {budget.Id} rejected by customer {normalizedDocument}."
                : $"Budget {budget.Id} rejected by customer {normalizedDocument}. Description: {description}",
            PerformedByUserId = null,
        };
        await dbContext.WorkOrderHistories.AddAsync(hist, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var customerEntity = await dbContext.Customers.FindAsync([wo.CustomerId], cancellationToken);
        if (customerEntity != null)
        {
            try
            {
                await emailService.SendWorkOrderStatusChanged(customerEntity, wo, WorkOrderStatus.PendingApproval,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send status changed email after budget rejection for WorkOrder {WorkOrderId}",
                    wo.Id);
            }
        }

        if (wo.AssignedToUserId != null)
        {
            var mechanic = await dbContext.Users.FindAsync([wo.AssignedToUserId], cancellationToken);
            EntityNotFoundException.ThrowIfNull(mechanic, wo.AssignedToUserId);

            try
            {
                await emailService.SendMechanicBudgetDecision(mechanic, wo, budget, approved: false, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send mechanic notification for rejected budget {BudgetId}", budget.Id);
            }
        }
    }
}
