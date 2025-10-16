using Mechanics.Application.Notification.Services;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Mechanics.Tests.Unit.Tests.Notifications;

[TestClass]
[TestCategory("Notification")]
[TestCategory("Email")]
public class EmailServiceTests
{
    public TestContext TestContext { get; set; }

    [TestMethod("Gera e-mail para nova ordem se serviço.")]
    public async Task It_ShouldSendEmail_WhenWorkOrderIsCreated()
    {
        var senderServiceStub = new Mock<IEmailSenderService>();
        senderServiceStub.Setup(service => service.SendAsync(It.IsAny<EmailMessage>(), TestContext.CancellationTokenSource.Token))
            .Verifiable(Times.Once());
        var service = CreateInstance(senderServiceStub.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());
        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            VehicleId = Guid.NewGuid(),
        };

        await service.SendWorkOrderCreated(customer, workOrder, TestContext.CancellationTokenSource.Token);

        senderServiceStub.Verify();
        var emailMessage = senderServiceStub.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        Assert.Contains($"{workOrder.AccessKey[..4]} {workOrder.AccessKey[4..]}", emailMessage.Body);
    }

    private static EmailService CreateInstance(IEmailSenderService senderService) =>
        new(new NullLoggerFactory().CreateLogger<EmailService>(), senderService);
}
