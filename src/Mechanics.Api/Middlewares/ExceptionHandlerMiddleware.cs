using Mechanics.Domain.Base.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Mechanics.Api.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (EntityNotFoundException e)
        {
            await WriteProblemDetails(context, StatusCodes.Status404NotFound, e);
        }
        catch (BusinessException e)
        {
            await WriteProblemDetails(context, StatusCodes.Status400BadRequest, e);
        }

        catch (Exception e)
        {
            await WriteProblemDetails(context, StatusCodes.Status500InternalServerError, e);
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
