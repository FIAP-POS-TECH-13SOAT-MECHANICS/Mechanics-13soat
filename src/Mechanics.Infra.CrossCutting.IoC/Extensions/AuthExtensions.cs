using Mechanics.Application.Options;
using Mechanics.Application.Utils.TokenGenerator;
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
        var configurationSection = configuration.GetSection(nameof(JwtOptions));
        services.Configure<JwtOptions>(configurationSection);
        var jwtOptions = configurationSection.Get<JwtOptions>();
        if (jwtOptions is null)
            throw new InvalidOperationException("JWT Public Key is not set.");

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
                    IssuerSigningKey = LoadSecurityKey(jwtOptions.PublicKey),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.EmployeesOnly, policy =>
                policy.RequireRole(RoleNames.Administrator, RoleNames.Attendant, RoleNames.Mechanic))
            .AddPolicy(PolicyNames.CustomersOnly, policy =>
                policy.RequireRole(RoleNames.CustomerAdmin, RoleNames.CustomerUser))
            .AddPolicy(PolicyNames.AllAuthenticated, policy => policy.RequireAuthenticatedUser());

        services.AddSingleton<IJwtTokenHandler, JwtTokenHandler>();

        return services;
    }

    private static RsaSecurityKey LoadSecurityKey(string publicKey)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);

        return new RsaSecurityKey(rsa);
    }
}
