using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;

namespace Mechanics.Application.Notification.Templates;

public static class WorkOrderEmailTemplates
{
    public static EmailMessage WorkOrderCreated(Customer customer, WorkOrder workOrder) => new()
    {
        Recipient = customer.Email,
        Subject = "Ordem de serviço criada - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Uma nova ordem de serviço foi criada para você:</p>

                <ul>
                <li><b>Veículo</b>: {workOrder.Vehicle}</li>
                <li><b>Data de criação</b>: {workOrder.CreationDate:G}</li>
                <li><b>Problema relatado</b>: // TODO [PROBLEMA_RELATADO]</li>
                </ul>

                <p>Você pode consultar o andamento do serviço informando seu documento e o código abaixo:<br/>
                <code style="font-weight: bold;">{workOrder.AccessKey[..4]} {workOrder.AccessKey[4..]}</code></p>
                """,
    };

    public static EmailMessage WorkOrderPendingApproval(Customer customer, WorkOrder workOrder, decimal estimatedTotal) => new()
    {
        Recipient = customer.Email,
        Subject = "Orçamento da OS disponível - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>O orçamento da sua ordem de serviço está pronto e aguarda sua aprovação.</p>
                <ul>
                <li><b>Ordem</b>: {workOrder.AccessKey}</li>
                <li><b>Valor estimado</b>: {estimatedTotal:C}</li>
                </ul>
                <p>Consulte usando seu documento e o código de acesso.</p>
                """
    };

    public static EmailMessage WorkOrderStatusChanged(Customer customer, WorkOrder workOrder, string previousStatus, string newStatus) => new()
    {
        Recipient = customer.Email,
        Subject = $"Atualização da OS {workOrder.AccessKey} - {newStatus} - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Sua ordem de serviço ({workOrder.AccessKey}) mudou de status:</p>
                <p><b>{previousStatus}</b> → <b>{newStatus}</b></p>
                <p>Data: {DateTime.UtcNow:G}</p>
                """
    };

    public static EmailMessage WorkOrderCancelled(Customer customer, WorkOrder workOrder) => new()
    {
        Recipient = customer.Email,
        Subject = $"OS {workOrder.AccessKey} cancelada - FIAP Mechanics",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Sua ordem de serviço ({workOrder.AccessKey}) foi cancelada.</p>
                """
    };

    /// <summary>
    ///     TODO: Implementar template de e-mail e lógica da pesquisa de satisfação futuramente.
    ///     Atualmente este método fornece um corpo simples para envio imediato. Em implementação
    ///     futura deverá:
    ///     - Gerar link único para a pesquisa (vinculado à `workOrder.AccessKey` e ao documento do cliente).
    ///     - Personalizar o conteúdo com informações do veículo e serviços realizados.
    /// </summary>
    /// <remarks>
    ///     Seguir fluxo de NOTIFICAÇÕES do Event Storming para implementação completa.
    /// </remarks>
    public static EmailMessage WorkOrderDeliveredSurvey(Customer customer, WorkOrder workOrder) => new()
    {
        Recipient = customer.Email,
        Subject = $"Pesquisa de satisfação - Ordem {workOrder.AccessKey}",
        Body = $"""
                <p>Olá, <b>{customer.Name}</b>,</p>
                <p>Seu veículo foi entregue. Por favor, responda nossa pesquisa rápida de satisfação.</p>
                """
    };
}
