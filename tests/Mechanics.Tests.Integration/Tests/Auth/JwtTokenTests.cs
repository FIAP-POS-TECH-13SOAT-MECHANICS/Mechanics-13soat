using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Options;
using Mechanics.Application.Utils;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using Mechanics.Tests.Integration.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Testing.Platform.Services;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
public class JwtTokenTests(TestContext testContext)
{
    private static readonly User TestUser = UserSeeds.GetSeeds().First();
    private static string TestUrl => "api/auth/users/db27b85d-b0f3-4300-bb45-7841f0d11617";

    [TestMethod("Deve retornar 401 se não estiver autenticado")]
    public async Task It_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var factory = TestProperties.Factory;
        var client = factory.CreateClient();

        var response = await client.GetAsync(TestUrl, testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod("Deve retornar 401 se o token estiver expirado")]
    public async Task It_ShouldReturnUnauthorized_WhenExpired()
    {
        var factory = TestProperties.Factory;
        var client = factory.CreateClient();
        var expiredToken = GetToken(new DateTime(2021, 4, 7));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken.AccessToken);

        var response = await client.GetAsync(TestUrl, testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod("Deve rejeitar renovação de token se o token estiver expirado")]
    public async Task It_ShouldRejectTokenRenewal_WhenExpired()
    {
        var factory = TestProperties.Factory;
        var client = factory.CreateClient();
        var expiredToken = GetToken(new DateTime(2021, 4, 7));

        var request = new RefreshTokenRequest { RefreshToken = expiredToken.RefreshToken };
        var response = await client.PostAsJsonAsync("api/auth/refresh", request, testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod("Deve retornar 200 se o token for válido")]
    public async Task It_ShouldReturnOk_WhenTokenIsValid()
    {
        var factory = TestProperties.Factory;
        var client = factory.CreateClient();
        var token = GetToken(DateTime.UtcNow);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var response = await client.GetAsync(TestUrl, testContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    private static TokenResponse GetToken(DateTime expirationDate)
    {
        var options = TestProperties.Factory.Server.Services.GetRequiredService<IOptions<JwtOptions>>();
        var tokenHandler = new JwtTokenHandler(options, new FakeTimeProvider(expirationDate));

        return tokenHandler.CreateTokenResponse(TestUser);
    }
}
