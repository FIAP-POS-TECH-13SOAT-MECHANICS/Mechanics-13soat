using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

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

    [TestMethod("Cadastro de cliente PJ com erro")]
    public async Task It_ShouldReturnBadRequest_WhenCnpjIsInvalid()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var invalidRequest = new CreateCustomerRequest
        {
            Name = "Empresa XYZ Ltda",
            Email = "contato@xyz.com",
            Document = new PersonalDocumentRequest
            {
                Type = DocumentType.Cnpj,
                Number = "12.345.678/0001-00",
            },
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/customers", invalidRequest, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, httpResponse.StatusCode);
    }
}
