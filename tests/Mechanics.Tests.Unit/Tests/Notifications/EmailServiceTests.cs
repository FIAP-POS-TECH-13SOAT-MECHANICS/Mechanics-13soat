using Mechanics.Application.Notification.Services;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Integrations.EmailSender;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mechanics.Tests.Unit.Tests.Notifications;

[TestClass]
[TestCategory("Notification")]
[TestCategory("Email")]
public class EmailServiceTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task It_ShouldSendEmail_WhenWorkOrderIsCreated()
    {
        // Arrange
        var senderServiceMock = new Mock<IEmailSenderService>();
        senderServiceMock
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateInstance(senderServiceMock.Object);
        var customer = CustomerMocks.CreateCustomerPf(Guid.NewGuid());

        var workOrder = new WorkOrder
        {
            Customer = customer,
            CustomerId = customer.Id,
            AccessKey = WorkOrder.GenerateNewAccessKey(Enumerable.Empty<WorkOrder>()),
            VehicleId = Guid.NewGuid(),
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now
        };

        // Act
        await service.SendWorkOrderCreated(customer, workOrder, CancellationToken.None);

        // Assert
        senderServiceMock.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);

        var emailMessage = senderServiceMock.Invocations[0].Arguments[0] as EmailMessage;
        Assert.IsNotNull(emailMessage);
        Assert.AreEqual(customer.Email, emailMessage.Recipient);
        Assert.IsNotNull(emailMessage.Subject);
        StringAssert.Contains(emailMessage.Body, $"{workOrder.AccessKey[..4]} {workOrder.AccessKey[4..]}");
    }

    private static EmailService CreateInstance(IEmailSenderService senderService) =>
        new EmailService(new NullLoggerFactory().CreateLogger<EmailService>(), senderService);
}
