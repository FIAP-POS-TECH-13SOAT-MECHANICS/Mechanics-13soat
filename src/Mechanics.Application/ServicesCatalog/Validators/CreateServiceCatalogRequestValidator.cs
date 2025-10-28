using FluentValidation;
using Mechanics.Application.ServicesCatalog.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.ServicesCatalog.Validators;

/// <summary>
///     Validador para criação de serviços no catálogo.
/// </summary>
public class CreateServiceCatalogRequestValidator : AbstractValidator<CreateServiceCatalogRequest>
{
    public CreateServiceCatalogRequestValidator(AppDbContext dbContext)
    {
        RuleFor(r => r.Name).NotEmpty()
            .MustAsync((roleName, cancellationToken) => dbContext.Roles.AnyAsync(r => r.Name == roleName, cancellationToken))
            .WithMessage("Invalid roleName.");

        RuleFor(r => r.Description).NotEmpty();

        RuleFor(r => r.BasePrice).GreaterThanOrEqualTo(0);

        RuleFor(r => r.AverageTime).GreaterThan(0);

        RuleFor(r => r.Status).IsInEnum();
    }
}
