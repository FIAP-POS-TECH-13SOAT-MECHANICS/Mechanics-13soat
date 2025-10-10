using FluentValidation;
using Mechanics.Application.Customers.Requests;

namespace Mechanics.Application.Customers.Validators;

public class CreateCustomerRequestValidator : AbstractValidator<CreateCustomerRequest>
{
    public CreateCustomerRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty();
        RuleFor(request => request.Email).NotEmpty();
        RuleFor(request => request.Document).NotNull().DependentRules(() =>
        {
            RuleFor(request => request.Document.Type).NotEmpty();
            RuleFor(request => request.Document.Number).NotEmpty();
        });
    }
}
