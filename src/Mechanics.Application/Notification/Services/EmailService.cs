using Mechanics.Application.Notification.Templates;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Microsoft.Extensions.Logging;

namespace Mechanics.Application.Notification.Services;

public class EmailService(ILogger<EmailService> logger, IEmailSenderService senderService) : IEmailService
{
    public async Task SendWorkOrderCreated(Customer customer, WorkOrder wordOrder, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending work order created notification to '{EmailAddress}'", customer.Email);

        var message = WorkOrderEmailTemplates.WorkOrderCreated(customer, wordOrder);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Work order created notification sent to '{EmailAddress}'", customer.Email);
    }

    public async Task SendUserPasswordCreationCode(User user, string passwordCreationCode,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending password creation code to '{EmailAddress}'", user.Email);

        var message = AuthEmailTemplates.UserPasswordCreationCode(user, passwordCreationCode);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Password creation code sent to '{EmailAddress}'", user.Email);
    }

    public async Task UserPasswordChanged(User user, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Sending password changed notification to '{EmailAddress}'", user.Email);

        var message = AuthEmailTemplates.UserPasswordChanged(user);
        await senderService.SendAsync(message, cancellationToken);

        logger.LogInformation("Password changed notification sent to '{EmailAddress}'", user.Email);
    }
}
