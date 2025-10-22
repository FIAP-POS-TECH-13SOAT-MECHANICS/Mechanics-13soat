using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.WorkOrders;

namespace Mechanics.Application.Notification.Services;

public interface IEmailService
{
    Task SendWorkOrderCreated(Customer customer, WorkOrder wordOrder, CancellationToken cancellationToken = default);

    Task SendUserPasswordCreationCode(User user, string passwordCreationCode, CancellationToken cancellationToken = default);
    Task UserPasswordChanged(User user, CancellationToken cancellationToken = default);
}
