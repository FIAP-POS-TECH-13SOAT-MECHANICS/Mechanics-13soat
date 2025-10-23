using Mechanics.Application.Options;
using Mechanics.Application.Utils;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("JwtTokenHandler")]
public class JwtTokenHandlerTests
{
    [TestMethod("Deve gerar token")]
    public void It_ShouldGenerateToken()
    {
        var user = UserMocks.CreateUser("47a67d29-ebe9eb190d97", "vmlK5NcD");
        var handler = CreateInstance(TimeProvider.System);

        var token = handler.CreateTokenResponse(user);

        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrWhiteSpace(token.AccessToken));
        Assert.IsFalse(string.IsNullOrWhiteSpace(token.RefreshToken));
    }

    [TestMethod("Deve extrair ID do token")]
    public void It_ShouldExtractUserIdFromToken()
    {
        var user = UserMocks.CreateUser("bb4c7b7e-f65865670979", "2aQuV6WCl9O");
        var handler = CreateInstance(TimeProvider.System);
        var token = handler.CreateTokenResponse(user);

        var userId = handler.GetUserId(token.AccessToken);

        Assert.IsNotNull(userId);
        Assert.AreEqual(user.Id, userId);
    }

    [TestMethod("Deve retornar true para refresh token válido")]
    public async Task It_ShouldReturnTrue_WhenValidateRefreshToken()
    {
        var user = UserMocks.CreateUser("bb4c7b7e-f65865670979", "2aQuV6WCl9O");
        var handler = CreateInstance(TimeProvider.System);
        var token = handler.CreateTokenResponse(user);

        var isValid = await handler.ValidateRefreshToken(token.RefreshToken, user.SecurityStamp);

        Assert.IsTrue(isValid);
    }

    [TestMethod("Deve retornar false para refresh token inválido")]
    public async Task It_ShouldReturnFalse_WhenValidateRefreshToken_WhitInvalidSecurityKey()
    {
        var user = UserMocks.CreateUser("e8f4ce07-052a104e696c", "CqeDMF5UH");
        var handler = CreateInstance(TimeProvider.System, "061a7dbf-1e64-440d-bb6a-d256c68b27bc");
        var invalidToken = CreateInstance(TimeProvider.System, "330e7d39-d3ea-4f05-a625-d1f14623b56f")
            .CreateTokenResponse(user);

        var isValid = await handler.ValidateRefreshToken(invalidToken.RefreshToken, user.SecurityStamp);

        Assert.IsFalse(isValid);
    }

    [TestMethod("Deve retornar false para securityStamp inválido")]
    public async Task It_ShouldReturnFalse_WhenValidateRefreshToken_WhitInvalidSecurityStamp()
    {
        var user = UserMocks.CreateUser("bb4c7b7e-f65865670979", "iKCg6koUvMr");
        var handler = CreateInstance(TimeProvider.System);
        var token = handler.CreateTokenResponse(user);

        var isValid = await handler.ValidateRefreshToken(token.RefreshToken, "DOo0rUV");

        Assert.IsFalse(isValid);
    }

    [TestMethod("Deve retornar false para refresh token expirado")]
    public async Task It_ShouldReturnFalse_WhenRefreshTokenIsExpired()
    {
        var user = UserMocks.CreateUser("bb4c7b7e-f65865670979", "iKCg6koUvMr");
        var handler = CreateInstance(new FakeTimeProvider(DateTimeOffset.UnixEpoch));
        var token = handler.CreateTokenResponse(user);

        var isValid = await handler.ValidateRefreshToken(token.RefreshToken, user.SecurityStamp);

        Assert.IsFalse(isValid);
    }

    private static JwtTokenHandler CreateInstance(TimeProvider timeProvider, string? secretKey = null)
    {
        var options = new OptionsWrapper<JwtOptions>(new JwtOptions
        {
            AccessTokenLifetime = 10,
            RefreshTokenLifetime = 120,
            SecretKey = secretKey ?? "9590d9fa-9917-44c2-9cd9-d6e0fb62391b",
        });

        return new JwtTokenHandler(options, timeProvider);
    }
}
