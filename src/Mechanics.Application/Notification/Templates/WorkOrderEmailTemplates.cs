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
}
