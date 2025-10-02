using Mechanics.Domain.Base.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Mechanics.Api.Middlewares;

public class DomainValidationMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainValidationException e)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";

            var response = new ProblemDetails
            {
                Extensions = e.ValidationResult.Errors
                    .ToDictionary(pair => pair.Key, object (pair) => pair.Value)!,
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, SerializerOptions));
        }
    }
}
