namespace Mechanics.Api.Infrastructure.Observability;

/// <summary>
/// Single source of truth for resource attributes used across logs, traces, and metrics.
/// Ensures service.name in Serilog logs matches service.name in OTel spans.
/// </summary>
public static class ObservabilityConstants
{
    public static string ResolveServiceName(IConfiguration configuration)
        => configuration["AppInfo:Name"] ?? "fiap-mechanics";

    public static string ResolveServiceVersion(IConfiguration configuration)
        => configuration["AppInfo:Version"] ?? "1.0.0";

    public static string ResolveDeploymentEnvironment(IHostEnvironment environment)
        => environment.EnvironmentName;

    public static string ResolveOtplEndpoint(IConfiguration configuration)
        => configuration["DataDog:OtplEndpoint"] ?? "";

    public static string ResolveDatadogApiKey(IConfiguration configuration)
        => configuration["DataDog:ApiKey"] ?? "";

    public static bool ResolveUseJsonLogs(IConfiguration contextConfiguration)
        => bool.TryParse(contextConfiguration["DataDog:UseJsonLogs"], out var result) && result;
}
