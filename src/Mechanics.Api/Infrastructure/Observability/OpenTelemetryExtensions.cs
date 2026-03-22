using System.Diagnostics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Mechanics.Api.Infrastructure.Observability;

public static class OpenTelemetryExtensions
{
    internal static readonly ActivitySource ActivitySource = new("Mechanics.Api");

    public static WebApplicationBuilder AddOpenTelemetryObservability(
        this WebApplicationBuilder builder)
    {
        var serviceName = ObservabilityConstants.ResolveServiceName(builder.Configuration);
        var serviceVersion = ObservabilityConstants.ResolveServiceVersion(builder.Configuration);
        var environment = ObservabilityConstants.ResolveDeploymentEnvironment(builder.Environment);

        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        var samplingRatio = builder.Environment.IsProduction() ? 0.1 : 1.0;

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment,
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(ActivitySource.Name)
                    .SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(samplingRatio)))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path.Value;
                            return path != null
                                && !path.StartsWith("/health",
                                    StringComparison.OrdinalIgnoreCase)
                                && !path.StartsWith("/swagger",
                                    StringComparison.OrdinalIgnoreCase);
                        };

                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            var clientIp = request.Headers["X-Forwarded-For"]
                                .FirstOrDefault()
                                ?.Split(',').FirstOrDefault()?.Trim()
                                ?? request.HttpContext.Connection
                                    .RemoteIpAddress?.ToString();

                            if (clientIp != null)
                                activity.SetTag("http.client_ip", clientIp);
                        };

                        options.RecordException = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = !builder.Environment.IsProduction();
                        options.Filter = (object obj) =>
                        {
                            if (obj is Microsoft.Data.SqlClient.SqlCommand cmd)
                            {
                                return cmd.CommandText == null
                                    || !cmd.CommandText.Contains("__EFMigrationsHistory");
                            }
                            return true;
                        };
                    });

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otlpEndpoint);
                        opts.TimeoutMilliseconds = 10_000;
                    });
                }

                if (builder.Environment.IsDevelopment())
                {
                    //tracing.AddConsoleExporter();
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (!string.IsNullOrEmpty(otlpEndpoint))
                {
                    metrics.AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otlpEndpoint);
                    });
                }

                if (builder.Environment.IsDevelopment())
                {
                    //metrics.AddConsoleExporter();
                }
            });

        return builder;
    }
}
