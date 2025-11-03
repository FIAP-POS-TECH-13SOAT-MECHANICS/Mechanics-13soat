using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class CreateWorkOrderRequestValidator : AbstractValidator<CreateWorkOrderRequest>
{
    public CreateWorkOrderRequestValidator()
    {
        RuleFor(r => r.VehicleId).NotEmpty().WithMessage("VehicleId is required.");

        RuleFor(r => r.ReportedProblem)
            .MaximumLength(1000)
            .WithMessage("ReportedProblem cannot exceed 1000 characters.");

        When(r => r.ProductIds != null, () =>
        {
            RuleForEach(r => r.ProductIds).NotEmpty().WithMessage("Product id cannot be empty.");
        });

        When(r => r.ServiceCatalogIds != null, () =>
        {
            RuleForEach(r => r.ServiceCatalogIds).NotEmpty().WithMessage("ServiceCatalog id cannot be empty.");
        });
    }
}
