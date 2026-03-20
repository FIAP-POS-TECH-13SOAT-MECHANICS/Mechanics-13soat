using Mechanics.Domain.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class AuthExtensions
{
    private const string JwtTokenIssuer = "fiap-mechanics";

    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtTokenIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = LoadSecurityKey(),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.EmployeesOnly, policy =>
                policy.RequireRole(RoleNames.Administrator, RoleNames.Attendant, RoleNames.Mechanic))
            .AddPolicy(PolicyNames.CustomersOnly, policy =>
                policy.RequireRole(RoleNames.CustomerAdmin, RoleNames.CustomerUser))
            .AddPolicy(PolicyNames.AllAuthenticated, policy => policy.RequireAuthenticatedUser());

        return services;
    }

    private static RsaSecurityKey LoadSecurityKey()
    {
        var publicKeyPath = Path.Combine(AppContext.BaseDirectory, "keys", "jwt-public.pem");
        if (!File.Exists(publicKeyPath))
            throw new InvalidOperationException("JWT Public Key is not available.");

        var publicKey = File.ReadAllText(publicKeyPath);

        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);

        return new RsaSecurityKey(rsa);
    }
}
