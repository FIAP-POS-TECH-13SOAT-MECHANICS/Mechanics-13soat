using Mechanics.Application.Auth.Models.Request;
using Mechanics.Application.Auth.Models.Response;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Mechanics.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IConfiguration configuration;

    public AuthService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var (Token, Expiration) = GenerateJwtToken();

        return await Task.FromResult(new LoginResponse
        {
            Token = Token,
            Expiration = Expiration
        });
    }

    private (string Token, DateTime Expiration) GenerateJwtToken()
    {
        var secretKey = configuration["JwtSettings:SecretKey"];
        var key = Encoding.ASCII.GetBytes(secretKey);

        var claims = new List<Claim>
        {
            new("id", "123456789"),
            new("username", "Teste"),
            new(ClaimTypes.Role, "Admin")
        };

        var expiration = DateTime.UtcNow.AddHours(2);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return (tokenString, expiration);
    }
}
