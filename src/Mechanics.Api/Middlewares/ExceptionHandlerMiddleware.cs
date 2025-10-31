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
        catch (Exception e)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";

            var response = new ProblemDetails
            {
                Status = 500,
                Type = e.GetType().FullName,
                Title = $"Application error: {e.Message}",
#if DEBUG
                Detail = e.StackTrace,
#endif
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
        }
    }
}
