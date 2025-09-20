using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Options;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mechanics.Application.Auth.Handlers;

public class LoginHandler(IOptions<JwtOptions> jwtOptions) : IRequestHandler<LoginRequest, LoginResponse?>
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public Task<LoginResponse?> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        if (request.Password != "12345")
            return Task.FromResult<LoginResponse?>(null);

        var key = Encoding.ASCII.GetBytes(_options.SecretKey);

        var claims = new List<Claim>
        {
            new("id", "123456789"),
            new("username", "Teste"),
            new(ClaimTypes.Role, "Admin"),
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
