namespace Mechanics.Api.Infrastructure.Observability;

/// <summary>
/// Single source of truth for resource attributes used across logs, traces, and metrics.
/// Ensures service.name in Serilog logs matches service.name in OTel spans.
/// </summary>
public static class ObservabilityConstants
{
    public static string ResolveServiceName(IConfiguration configuration)
        => configuration["Application:Name"] ?? "mechanics-api";

    public static string ResolveServiceVersion()
        => Environment.GetEnvironmentVariable("SERVICE_VERSION") ?? "0.0.0";

    public static string ResolveDeploymentEnvironment(IHostEnvironment environment)
        => environment.EnvironmentName;
}
