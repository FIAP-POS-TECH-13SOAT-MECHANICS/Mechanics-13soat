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
    
    private static async Task WriteProblemDetails(HttpContext context, int statusCode, Exception exception)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var response = new ProblemDetails
        {
            Status = statusCode,
            Type = exception.GetType().FullName,
            Title = $"Application error: {exception.Message}",
#if DEBUG
            Detail = exception.StackTrace,
#endif
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
    }    
}
