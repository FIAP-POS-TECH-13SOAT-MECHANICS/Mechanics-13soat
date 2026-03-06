using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Compact;

namespace Mechanics.Api.Infrastructure.Observability;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddStructuredLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithSpan()
                .WriteTo.Console(new CompactJsonFormatter());
        });

        return builder;
    }
}
