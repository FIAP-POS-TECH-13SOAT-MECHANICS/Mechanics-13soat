using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Mechanics.Api.Middlewares;

public class RequestValidationFilter(ILogger<RequestValidationFilter> logger, IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogDebug("Validating request for action: {Action}", context.ActionDescriptor.DisplayName);
        var errors = new Dictionary<string, string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var argumentType = argument.GetType();
            if (argumentType.IsPrimitive || argumentType == typeof(string))
                return;

            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            var validator = serviceProvider.GetService(validatorType);
            if (validator is null)
                continue;

            var cancellationToken = context.HttpContext.RequestAborted;
            var validateMethod = validatorType.GetMethod("ValidateAsync", [argumentType, cancellationToken.GetType()]);
            if (validateMethod?.Invoke(validator, [argument, cancellationToken]) is not Task<ValidationResult> validationResultTask)
                return;

            var validationResult = await validationResultTask;
            if (validationResult is not { IsValid: false })
                continue;

            foreach (var error in validationResult.Errors)
            {
                var camelCasePropertyName = $"{error.PropertyName[0].ToString().ToLower()}{error.PropertyName[1..]}";
                if (errors.TryGetValue(camelCasePropertyName, out var value))
                {
                    var separator = value.EndsWith('.') ? " " : ". ";
                    errors[camelCasePropertyName] = string.Concat(value, separator, error.ErrorMessage);
                    continue;
                }

                errors[camelCasePropertyName] = error.ErrorMessage;
            }
        }

        if (errors.Count == 0)
        {
            await next();
            return;
        }

        logger.LogInformation("Validating failed for action: {Action}", context.ActionDescriptor.DisplayName);

        var response = new ProblemDetails
        {
            Status = 400,
            Extensions = new Dictionary<string, object?> { { "errors", errors } },
        };
        context.Result = new BadRequestObjectResult(response);
    }
}
