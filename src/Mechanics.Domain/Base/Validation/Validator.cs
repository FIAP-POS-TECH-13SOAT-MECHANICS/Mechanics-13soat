namespace Mechanics.Domain.Base.Validation;

public static class Validator
{
    public static void ValidateAndThrow(IValidatable validatable)
    {
        var builder = new ValidationBuilder();
        validatable.Validate(builder);
        builder.Build().ThrowIfInvalid();
    }
}

public class DomainValidationException(ValidationResult validationResult) : Exception("One or more validations failed.")
{
    public ValidationResult ValidationResult { get; } = validationResult;
}
