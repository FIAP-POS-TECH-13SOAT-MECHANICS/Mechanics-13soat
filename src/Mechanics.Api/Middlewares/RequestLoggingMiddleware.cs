using System.Diagnostics;

namespace Mechanics.Api.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var method = context.Request.Method;
        var endpoint = $"{context.Request.Path}{context.Request.QueryString}";
        var scheme = context.Request.Scheme;

        var endpointName = context.GetEndpoint()?.DisplayName;

        var clientIp =
            context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?.Split(',').FirstOrDefault()?.Trim()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        var userAgent = context.Request.Headers.UserAgent.ToString();

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(ex,
                "Unhandled exception on HTTP {Method} {Endpoint} from {ClientIp} after {Duration}ms",
                method,
                endpoint,
                clientIp,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;

            var level =
                statusCode >= 500 ? LogLevel.Error :
                statusCode >= 400 ? LogLevel.Warning :
                LogLevel.Information;

            logger.Log(
                level,
                "HTTP {Method} {Endpoint} ({EndpointName}) {Scheme} from {ClientIp} responded {StatusCode} in {Duration}ms | UA: {UserAgent}",
                method,
                endpoint,
                endpointName,
                scheme,
                clientIp,
                statusCode,
                stopwatch.ElapsedMilliseconds,
                userAgent);
        }
    }
}
