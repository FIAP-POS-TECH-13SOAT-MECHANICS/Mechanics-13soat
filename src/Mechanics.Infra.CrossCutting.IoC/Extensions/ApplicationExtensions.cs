using Mechanics.Application.Auth.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatR(config =>
        {
            config.LicenseKey = configuration.GetValue<string>("MediatRLicenseKey");
            config.RegisterServicesFromAssembly(typeof(LoginHandler).Assembly);
        });

        return services;
    }
}
