using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class ChangeStatusRequestValidator : AbstractValidator<ChangeStatusRequest>
{
    public ChangeStatusRequestValidator()
    {
        RuleFor(r => r.NewStatus).IsInEnum().WithMessage("Invalid status value.");
        RuleFor(r => r.PerformedBy).NotEmpty().WithMessage("PerformedBy is required.");
    }
}
