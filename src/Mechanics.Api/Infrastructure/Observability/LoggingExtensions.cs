using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;

namespace Mechanics.Api.Infrastructure.Observability;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        var serviceName = builder.Configuration["Application:Name"] ?? "mechanics-api";
        var serviceVersion = Environment.GetEnvironmentVariable("SERVICE_VERSION") ?? "0.0.0";
        var deploymentEnvironment = builder.Environment.EnvironmentName;

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithSpan()
                .Enrich.WithProperty("service.name", serviceName)
                .Enrich.WithProperty("service.version", serviceVersion)
                .Enrich.WithProperty("deployment.environment", deploymentEnvironment)
                .WriteTo.Console(new CompactJsonFormatter());
        });

        return builder;
    }
}
