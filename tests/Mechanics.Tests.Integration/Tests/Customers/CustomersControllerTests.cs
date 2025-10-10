using System.Net;
using System.Net.Http.Json;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Tests.Integration.Helpers;

namespace Mechanics.Tests.Integration.Tests.Customers;

[TestClass]
[TestCategory("Integration")]
[TestCategory("Customers")]
public class CustomersControllerTests
{
    public TestContext TestContext { get; set; }

    [TestMethod("Cadastro de cliente")]
    public async Task It_ShouldCreateCustomer()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateCustomerRequest
        {
            Name = "Joao da Silva",
            Email = "joao@example.com",
            Document = new PersonalDocumentRequest
            {
                Type = DocumentType.Cpf,
                Number = "111.444.777-35",
            },
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/customers", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }
}
