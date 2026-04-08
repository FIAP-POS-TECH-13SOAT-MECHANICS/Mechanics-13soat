using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;
using Serilog.Sinks.OpenTelemetry;

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
            var loggerConfiguration = configuration
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
                .WriteTo.OpenTelemetry(options =>
                {
                    var otlpEndpoint = ObservabilityConstants.ResolveOtplEndpoint(context.Configuration);
                    if (!string.IsNullOrEmpty(otlpEndpoint))
                        options.Endpoint = $"{otlpEndpoint}/v1/logs";
                    options.Protocol = OtlpProtocol.HttpProtobuf;

                    options.Headers = new Dictionary<string, string>
                    {
                        ["DD-API-KEY"] = ObservabilityConstants.ResolveDatadogApiKey(context.Configuration),
                    };

                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = serviceName,
                        ["service.version"] = serviceVersion,
                        ["deployment.environment"] = deploymentEnvironment,
                    };
                });

            if (ObservabilityConstants.ResolveUseJsonLogs(context.Configuration))
                loggerConfiguration.WriteTo.Console(new RenderedCompactJsonFormatter());
            else
                loggerConfiguration.WriteTo.Console();
        });

        return builder;
    }
}
