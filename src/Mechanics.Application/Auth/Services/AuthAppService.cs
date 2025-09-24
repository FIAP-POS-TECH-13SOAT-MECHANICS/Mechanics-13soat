using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Options;
using Mechanics.Application.Utils;
using Mechanics.Domain.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mechanics.Application.Auth.Services;

public class AuthAppService(IOptions<JwtOptions> jwtOptions) : IAppService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public Task<LoginResponse?> Login(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Password != "12345")
            return Task.FromResult<LoginResponse?>(null);

        var key = Encoding.ASCII.GetBytes(_options.SecretKey);

        var claims = new List<Claim>
        {
            new("id", "1b0359c8-3e7c-42bb-b8cf-36d58957fb31"),
            new("username", "Administrator"),
            new(ClaimTypes.Role, RoleNames.Administrator),
        };

        var expiration = DateTime.UtcNow.AddMinutes(_options.AccessTokenLifetime);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Task.FromResult(new LoginResponse
        {
            AccessToken = tokenHandler.WriteToken(token),
            ExpirationDate = expiration,
        })!;
    }
}
