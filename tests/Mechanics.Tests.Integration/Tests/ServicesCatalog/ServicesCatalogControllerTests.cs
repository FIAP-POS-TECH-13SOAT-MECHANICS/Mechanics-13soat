using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.ServicesCatalog;

[TestClass]
[TestCategory("Integration")]
[TestCategory("ServicesCatalog")]
public class ServiceCatalogControllerTests
{
    public TestContext TestContext { get; set; }

    [TestMethod("Verifica se rota está acessível")]
    public async Task It_ShouldReachServiceCatalogEndpoint()
    {
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var response = await client.GetAsync("api/service-catalog");

        Console.WriteLine($"Status: {response.StatusCode}");
        Assert.AreNotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }


    [TestMethod("Cadastro de serviço")]
    public async Task It_ShouldCreateServiceCatalog()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateServiceCatalogRequest
        {
            Name = $"Alinhamento {Guid.NewGuid():N}",
            Description = "Serviço de alinhamento de rodas",
            BasePrice = 150.00m,
            AverageTime = 40,
            Status = ServiceCatalogStatusType.Active
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/service-catalog", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content.CreatedId);
    }

    [TestMethod("Falha ao cadastrar serviço com nome duplicado")]
    public async Task It_ShouldFailToCreateServiceCatalog_WhenNameIsDuplicated()
    {
        // Arrange
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var name = $"Balanceamento {Guid.NewGuid():N}";
        var request = new CreateServiceCatalogRequest
        {
            Name = name,
            Description = "Balanceamento de rodas dianteiras",
            BasePrice = 199.90m,
            AverageTime = 45,
            Status = ServiceCatalogStatusType.Active
        };

        // Act - primeiro cadastro (deve funcionar)
        var firstResponse = await client.PostAsJsonAsync("api/service-catalog", request, TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act - segundo cadastro com mesmo nome (deve falhar)
        var duplicateRequest = new CreateServiceCatalogRequest
        {
            Name = name, // mesmo nome
            Description = "Outro serviço com nome repetido",
            BasePrice = 149.90m,
            AverageTime = 30,
            Status = ServiceCatalogStatusType.Active
        };

        var secondResponse =
            await client.PostAsJsonAsync("api/service-catalog", duplicateRequest, TestContext.CancellationTokenSource.Token);

        // Assert
        var raw = await secondResponse.Content.ReadAsStringAsync(TestContext.CancellationTokenSource.Token);
        Console.WriteLine($"Status: {secondResponse.StatusCode}, Body: {raw}");

        Assert.AreEqual(HttpStatusCode.BadRequest, secondResponse.StatusCode,
            $"Esperado BadRequest, mas veio: {secondResponse.StatusCode}");
        Assert.IsTrue(raw.Contains("Service name must be unique.", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod("Sugestões com tipo de veículo vazio")]
    public async Task It_ShouldFailToSuggestServices_WhenVehicleTypeIsEmpty()
    {
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var response = await client.GetAsync("api/service-catalog/suggestions?vehicleType=",
            TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
