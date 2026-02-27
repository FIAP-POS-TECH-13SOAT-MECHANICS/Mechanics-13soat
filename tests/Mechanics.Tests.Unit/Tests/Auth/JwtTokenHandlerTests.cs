using Mechanics.Application.Options;
using Mechanics.Application.Utils.TokenGenerator;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using System.Security.Cryptography;

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
        var handler = CreateInstance(TimeProvider.System);
        var invalidToken = CreateInstance(TimeProvider.System, new RSACryptoServiceProvider().ExportRSAPrivateKeyPem())
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

    private static JwtTokenHandler CreateInstance(TimeProvider timeProvider, string? privateKey = null)
    {
        var options = new OptionsWrapper<JwtOptions>(new JwtOptions
        {
            AccessTokenLifetime = 10,
            RefreshTokenLifetime = 120,
            PublicKey =
                "-----BEGIN PUBLIC KEY-----\nMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAsbXaHUJkwxE8SSkYMhom\nXchxOJjryFTphChTSz4ZdOew683kTzwvmxUIkBf9hD4d++72IVpSHDavLuPStG6j\nu61BMtYW/LNRQSy1iPcabxUiZzX5TaIfIk4wjYSickEzzyE9zNhnvGpVUYi9kgFJ\nl0r+3N3lVE3qAoGwjLXh5lEjuKeQIDqpyghTIPVGcWHApapIVY3BIkm0ewAopPgr\nflOY8MfyVK2GiKR3i/qz3xUKj0bnVizzR85yI3Ihtia5fDyMvLGeBwOeXJW6EbO4\nhcds/4dvUi9YJxJConQ2qQnttySpDJMYvgLTCqp4LrvJn4EJ8bCUBwgEqGkPfG0T\nEwIDAQAB\n-----END PUBLIC KEY-----",
            PrivateKey = privateKey ??
                         "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCxtdodQmTDETxJ\nKRgyGiZdyHE4mOvIVOmEKFNLPhl057DrzeRPPC+bFQiQF/2EPh377vYhWlIcNq8u\n49K0bqO7rUEy1hb8s1FBLLWI9xpvFSJnNflNoh8iTjCNhKJyQTPPIT3M2Ge8alVR\niL2SAUmXSv7c3eVUTeoCgbCMteHmUSO4p5AgOqnKCFMg9UZxYcClqkhVjcEiSbR7\nACik+Ct+U5jwx/JUrYaIpHeL+rPfFQqPRudWLPNHznIjciG2Jrl8PIy8sZ4HA55c\nlboRs7iFx2z/h29SL1gnEkKidDapCe23JKkMkxi+AtMKqnguu8mfgQnxsJQHCASo\naQ98bRMTAgMBAAECggEAQ0l+bS669Us83tyy/yF8nmzrRclNGXzhMDuOkkJQpD2n\n9Rjv1fqNcfRsWhbjVGOqub4YrrdCuNER0rjLiqsLzt+CfMuoR4VkAj/5+oLJnP7N\nGHRM0ZLNpBEj/CmY0pcSlV5aRoo3+RTv+h/C25omMaVhS+Ku1xVrQgi1/wlNIAW9\nWg3wTrNBayOmJJdCQb3TaLcZoHwlgrlXfLDAVhO2YwkO2/hkG4qheLUnhyusHML4\ntcJZ0BTPBUsdEpug0Np4hp9QiH6eYwfkrx6iTwKqsYyekXW6KOdOYGm/JK5qm8eQ\ngaCYI1WOGsx2+odEB7YS01eKHHWIFg+SOK071IDRhQKBgQDvlHAxJdPjbfurnzn2\nlmelrqwOPox4F5RVdb0fB6SvX+VJX3oKb1V7/NpoyQ1ZPXcKxNex9zPJ1+jfGOvK\nfZ5Mk1NmvfRTGEy3ovs8tMEYoyBCqzRCESAYXBTcUQLfgAe6iWilxDX/yAZxkMLF\nlRR/nqT0qE8gYYReMWjlR8TMPwKBgQC94+EZrN84JaOmSsbFIPVbsL9iog3GBzUh\nDxyZCsoS7cSv7IKl/UKnpJlq+Pk3E3Wdh0XmeIFoBCeOJMSbZd8GFsZMKcIDhRCg\nkoVnRhtvSXreupmh5Zfo3QmRRZahvd1CMUGpNam1dZM3VpwRAdbXw5C4o45f65vB\n0l/xkg/ULQKBgFcjuW7W4Gu/TCOPJYkACbDkiGYh7/uaL/Spf2Ey6X50NbRrSrtS\n5VfUjyg/wGAuEAdVs1JipG6M3oGO0exzpkkZ6OLcwmoa04STfigFYppwcsJs/PPu\nwKq7i0bbHF1oductJWftpupiuZ1C1uUApVUJwdvPAAC6F2gfNpT2dqkHAoGBAJHo\n5nDKKwODuInw72dN+fqoh4cMqrS9mQ98Ayd96OQ+m2HqxEEzp+IVUmWmRikR5NBU\nv1tmwVjhyFsq2X/m+UY+fcTMWW5G8w3PSH6gmjgbKDS5AZArUVz3a4CbDjHeKIPS\nQXf56huq6qIdBPL0jNdiSHP4CVCfRCHcuyhuaYdJAoGAMyhSTk+0GBuu72ILaXrt\nkK/m0pbYdVQsPkl/+qNzQWrlUoh532ECuAhgi4s+yt6q2Ne5OFWfuWd/dqNq7qmg\n7+9olVkpegQPteEROPsA5SMLW33kB4xtfwBf6wcuTiHlFTA2F7jQf0A/a4wn4l3z\n6Boctf6HhD7TQvgnTJGr7aw=\n-----END PRIVATE KEY-----",
        });

        return new JwtTokenHandler(options, timeProvider);
    }
}
