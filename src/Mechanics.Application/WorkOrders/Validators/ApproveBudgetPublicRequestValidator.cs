using FluentValidation;
using Mechanics.Application.WorkOrders.Requests;

namespace Mechanics.Application.WorkOrders.Validators;

public class ApproveBudgetPublicRequestValidator : AbstractValidator<BudgetReviewRequest>
{
    public ApproveBudgetPublicRequestValidator()
    {
        RuleFor(request => request.AccessKey).NotEmpty().Length(8);
    }
}
