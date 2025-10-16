using Mechanics.Application.Notification.Templates;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Notification.Services;

public class EmailService(ILogger<EmailService> logger, IEmailSenderService senderService) : IEmailService
{
    public async Task SendWorkOrderCreated(Customer customer, WorkOrder wordOrder, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending work order created notification to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderCreated(customer, wordOrder);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order created notification sent to '{EmailAddress}'", customer.Email);
    }
}
