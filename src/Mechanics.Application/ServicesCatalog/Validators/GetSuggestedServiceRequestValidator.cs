using FluentValidation;
using Mechanics.Application.ServicesCatalog.Requests;

namespace Mechanics.Application.ServicesCatalog.Validators;

public class GetSuggestedServiceRequestValidator : AbstractValidator<GetSuggestedServiceRequest>
{
    public GetSuggestedServiceRequestValidator()
    {
        RuleFor(request => request.VehicleType).NotEmpty();
    }
}
