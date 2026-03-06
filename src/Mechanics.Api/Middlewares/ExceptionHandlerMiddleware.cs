using Mechanics.Domain.Base.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Mechanics.Api.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            if (context.Response.HasStarted)
            {
                logger.LogError(e,
                    "Response already started, cannot handle exception | {Method} {Endpoint}",
                    context.Request.Method,
                    context.Request.Path);

                throw;
            }

            await HandleException(context, e);
        }
    }
    private async Task HandleException(HttpContext context, Exception exception)
    {
        var method = context.Request.Method;
        var endpoint = $"{context.Request.Path}{context.Request.QueryString}";

        switch (exception)
        {
            case EntityNotFoundException e:
                logger.LogWarning(e,
                    "Entity not found | {Method} {Endpoint}",
                    method,
                    endpoint);

                await WriteProblemDetails(context, StatusCodes.Status400BadRequest, e);
                break;

            case BusinessException e:
                logger.LogWarning(e,
                    "Business rule violation | {Method} {Endpoint}",
                    method,
                    endpoint);

                await WriteProblemDetails(context, StatusCodes.Status400BadRequest, e);
                break;

            default:
                logger.LogError(exception,
                    "Unhandled exception | {Method} {Endpoint}",
                    method,
                    endpoint);

                await WriteProblemDetails(context, StatusCodes.Status500InternalServerError, exception);
                break;
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, int statusCode, Exception e)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetails
        {
            Status = statusCode,
            Type = e.GetType().FullName,
            Title = $"Application error: {e.Message}",
            Extensions =
            {
                ["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
            },
#if DEBUG
            Detail = JsonSerializer.Serialize(new ExceptionDetails(e), SerializerOptions),
#endif
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }


    public class ExceptionDetails(Exception e)
    {
        public string Name { get; } = e.GetType().Name;
        public string Message { get; } = e.Message;
        public ExceptionDetails? InnerException { get; } = e.InnerException != null ? new ExceptionDetails(e.InnerException) : null;
    }
}
