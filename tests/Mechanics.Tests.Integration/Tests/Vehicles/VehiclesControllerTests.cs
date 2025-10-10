using System.Net;
using System.Net.Http.Json;
using Mechanics.Application.Customers.Requests;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Vehicles;
using Mechanics.Tests.Integration.Helpers;

namespace Mechanics.Tests.Integration.Tests.Vehicles;

[TestClass]
[TestCategory("Integration")]
[TestCategory("Vehicles")]
public class VehiclesControllerTests
{
    public TestContext TestContext { get; set; }

    private async Task<(HttpClient client, Guid ownerId)> GetClientAndOwner()
    {
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var customerRequest = new CreateCustomerRequest
        {
            Name = "Owner Test",
            Email = $"owner_{Guid.NewGuid():N}@example.com",
            Document = new PersonalDocumentRequest
            {
                Type = DocumentType.Cpf,
                Number = "67273958026",
            },
        };

        var createdCustomer = await client.PostAsJsonAsync("api/customers", customerRequest, TestContext.CancellationTokenSource.Token);
        var customerResponse = await createdCustomer.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        return (client, customerResponse!.CreatedId);
    }

    [TestMethod("Cadastro de veículo")]
    public async Task It_ShouldCreateVehicle()
    {
        // Arrange
        var (client, ownerId) = await GetClientAndOwner();
        var request = new CreateVehicleRequest
        {
            Manufacturer = "Ford",
            Model = "Fiesta",
            Color = VehicleColor.Black,
            Year = "2020",
            LicensePlate = "ABC1D23",
            Chassis = "9BW9ZZ377VT004251",
            OwnerId = ownerId,
        };

        // Act
        var httpResponse = await client.PostAsJsonAsync("api/vehicles", request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, httpResponse.StatusCode);
        var content = await httpResponse.Content.ReadFromJsonAsync<CreateItemResponse>(TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.AreNotEqual(Guid.Empty, content!.CreatedId);
    }
}
