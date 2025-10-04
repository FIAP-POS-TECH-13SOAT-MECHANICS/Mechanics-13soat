using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Integration.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Users")]
public class CreateUserTest(TestContext testContext)
{
    [TestMethod("Falha quando RoleId é inválido.")]
    public async Task It_ShouldFail_WhenRoleIdIsInvalid()
    {
        var factory = TestProperties.Factory;
        var client = await factory.GetAuthenticatedClient(RoleNames.Administrator);

        var request = new CreateUserRequest
        {
            FullName = "MARIA FERNANDA SOUZA",
            UserName = "maria.souza",
            RoleId = new Guid("f2d59afa-6e85-4557-8ff1-733343ba83f8"),
        };

        var response = await client.PostAsJsonAsync("api/auth/users", request,
            testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
