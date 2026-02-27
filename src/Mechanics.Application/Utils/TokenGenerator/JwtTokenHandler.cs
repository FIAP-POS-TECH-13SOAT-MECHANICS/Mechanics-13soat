using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Options;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data.Seeds;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Mechanics.Application.Utils.TokenGenerator;

public class JwtTokenHandler(IOptions<JwtOptions> jwtOptions, TimeProvider timeProvider) : IJwtTokenHandler
{
    private readonly JwtOptions _options = jwtOptions.Value;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private const string JwtTokenIssuer = "fiap-mechanics";
    private readonly Dictionary<Guid, string> _roles = RoleSeeds.GetSeeds().ToDictionary(r => r.Id, role => role.Name);

    public TokenResponse CreateTokenResponse(User user)
    {
        var expiration = timeProvider.GetUtcNow().AddMinutes(_options.AccessTokenLifetime);

        return new TokenResponse
        {
            AccessToken = GenerateAccessToken(user, expiration),
            RefreshToken = GenerateRefreshToken(user),
            ExpirationDate = expiration,
        };
    }

    public Guid? GetUserId(string token)
    {
        var subject = _tokenHandler.ReadJsonWebToken(token).Subject;
        return Guid.TryParse(subject, out var userId) ? userId : null;
    }

    public async Task<bool> ValidateRefreshToken(string refreshToken, string securityStamp)
    {
        var userId = _tokenHandler.ReadJsonWebToken(refreshToken).Subject;

        var refreshTokenKey = Encoding.ASCII.GetBytes($"{userId}:{securityStamp}:{_options.PrivateKey}");
        var validationResult = await _tokenHandler.ValidateTokenAsync(refreshToken, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(refreshTokenKey),
            ValidateIssuer = true,
            ValidIssuer = JwtTokenIssuer,
            ValidateAudience = false,
            ValidateLifetime = true,
        });

        return validationResult.IsValid;
    }

    private string GenerateAccessToken(User user, DateTimeOffset expiration)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("userName", user.CpfNumber),
            new("role", _roles[user.RoleId]),
        };

        var rsa = RSA.Create();
        rsa.ImportFromPem(_options.PrivateKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtTokenIssuer,
            Subject = new ClaimsIdentity(claims),
            Expires = expiration.UtcDateTime,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256),
            IssuedAt = timeProvider.GetUtcNow().UtcDateTime,
            NotBefore = timeProvider.GetUtcNow().UtcDateTime,
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }

    private string GenerateRefreshToken(User user)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
        };

        var key = Encoding.ASCII.GetBytes($"{user.Id}:{user.SecurityStamp}:{_options.PrivateKey}");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtTokenIssuer,
            Subject = new ClaimsIdentity(claims),
            Expires = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.RefreshTokenLifetime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            IssuedAt = timeProvider.GetUtcNow().UtcDateTime,
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }
}
