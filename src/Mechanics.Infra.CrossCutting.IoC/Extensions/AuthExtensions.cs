using Mechanics.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(nameof(JwtOptions));
        services.Configure<JwtOptions>(configurationSection);
        var jwtOptions = configurationSection.Get<JwtOptions>();
        if (jwtOptions is null)
            throw new InvalidOperationException("JWT Secret Key is not set.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                };
            });

        services.AddAuthorization();

        return services;
    }
}
