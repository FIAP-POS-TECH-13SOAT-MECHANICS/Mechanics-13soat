using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class ApproveBudgetPublicRequestValidator : AbstractValidator<ApproveBudgetPublicRequest>
{
    public ApproveBudgetPublicRequestValidator()
    {
        RuleFor(x => x.Document).NotEmpty().WithMessage("Document is required.");
        RuleFor(x => x.AccessKey).NotEmpty().Length(8).WithMessage("AccessKey is required and must be 8 characters.");
    }
}
