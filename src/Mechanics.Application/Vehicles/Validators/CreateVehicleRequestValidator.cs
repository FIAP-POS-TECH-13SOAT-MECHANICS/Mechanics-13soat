using FluentValidation;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Vehicles.Validators;

public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequest>
{
    public CreateVehicleRequestValidator(AppDbContext dbContext)
    {
        RuleFor(r => r.Manufacturer).NotEmpty();
        RuleFor(r => r.Model).NotEmpty();
        RuleFor(r => r.Color).IsInEnum();
        RuleFor(r => r.Year).NotEmpty();
        RuleFor(r => r.LicensePlate).NotEmpty();
        RuleFor(r => r.Chassis).NotEmpty().MaximumLength(17);

        RuleFor(r => r.OwnerId).NotEmpty()
            .MustAsync(async (ownerId, ct) => await dbContext.Customers.AnyAsync(c => c.Id == ownerId, ct))
            .WithMessage("Invalid ownerId.");
    }
}
