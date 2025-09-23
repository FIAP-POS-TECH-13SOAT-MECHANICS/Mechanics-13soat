using Mechanics.Application.Auth.Responses;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Roles")]
public class GetRolesTest(TestContext testContext)
{
    [TestMethod]
    public async Task It_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var factory = TestProperties.Factory;
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/auth/roles", testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task It_ShouldReturnNotEmptyList()
    {
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var response = await client.GetAsync("api/auth/roles", testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsNotNull(response.Content);
        var content = await response.Content.ReadFromJsonAsync<GetRolesResponse>(testContext.CancellationTokenSource.Token);
        Assert.IsNotNull(content);
        Assert.IsNotEmpty(content.Items);
    }
}
