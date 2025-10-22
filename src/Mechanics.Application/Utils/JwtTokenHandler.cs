using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Options;
using Mechanics.Application.Utils.TokenGenerator;
using Mechanics.Domain.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Mechanics.Application.Utils;

public class JwtTokenHandler(IOptions<JwtOptions> jwtOptions) : IJwtTokenHandler
{
    private readonly JwtOptions _options = jwtOptions.Value;
    private readonly JsonWebTokenHandler _tokenHandler = new();
    private const string JwtTokenIssuer = "fiap-mechanics";

    public TokenResponse CreateTokenResponse(User user)
    {
        var expiration = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetime);

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
        var refreshTokenKey = Encoding.ASCII.GetBytes($"{_options.SecretKey}:{securityStamp}");
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

    private string GenerateAccessToken(User user, DateTime expiration)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
            new("userName", user.UserName),
            new("role", RoleNames.Administrator),
        };

        var key = Encoding.ASCII.GetBytes(_options.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtTokenIssuer,
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }

    private string GenerateRefreshToken(User user)
    {
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()),
        };

        var key = Encoding.ASCII.GetBytes($"{_options.SecretKey}:{user.SecurityStamp}");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = JwtTokenIssuer,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.RefreshTokenLifetime),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            IssuedAt = DateTime.UtcNow,
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }
}
