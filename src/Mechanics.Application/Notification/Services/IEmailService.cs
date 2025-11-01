using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.Notification.Services;

public interface IEmailService
{
    Task SendWorkOrderCreated(Customer customer, WorkOrder wordOrder, CancellationToken cancellationToken = default);
    Task SendWorkOrderPendingApproval(Customer customer, WorkOrder workOrder, decimal estimatedTotal, CancellationToken cancellationToken = default);
    Task SendWorkOrderStatusChanged(Customer customer, WorkOrder workOrder, string previousStatus, string newStatus, CancellationToken cancellationToken = default);
    Task SendWorkOrderCancelled(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia pesquisa pós-entrega para o cliente associada à ordem de serviço.
    /// </summary>
    /// <remarks>
    /// AVISO: lógica de envio ainda não implementada. Seguir fluxo do Event Storming de Notificações;
    /// </remarks>
    Task SendWorkOrderDeliveredSurvey(Customer customer, WorkOrder workOrder, CancellationToken cancellationToken = default); 

    Task SendUserPasswordCreationCode(User user, string passwordCreationCode, CancellationToken cancellationToken = default);
    Task UserPasswordChanged(User user, CancellationToken cancellationToken = default);
}   
