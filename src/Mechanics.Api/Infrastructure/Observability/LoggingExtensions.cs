using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;

namespace Mechanics.Api.Infrastructure.Observability;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        var serviceName = ObservabilityConstants.ResolveServiceName(builder.Configuration);
        var serviceVersion = ObservabilityConstants.ResolveServiceVersion(builder.Configuration);
        var deploymentEnvironment = ObservabilityConstants.ResolveDeploymentEnvironment(builder.Environment);

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            var datadogApiKey = context.Configuration["Datadog:ApiKey"]
                    ?? Environment.GetEnvironmentVariable("DD_API_KEY");
                    
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithSpan()
                .Enrich.With(new DatadogTraceEnricher())
                .Enrich.WithProperty("service.name", serviceName)
                .Enrich.WithProperty("service.version", serviceVersion)
                .Enrich.WithProperty("deployment.environment", deploymentEnvironment)
                .WriteTo.Console(new RenderedCompactJsonFormatter());
        });

        return builder;
    }
}
