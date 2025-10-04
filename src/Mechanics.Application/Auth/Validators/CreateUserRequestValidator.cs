using FluentValidation;
using Mechanics.Application.Auth.Requests;
using Mechanics.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Application.Auth.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(AppDbContext dbContext)
    {
        RuleFor(request => request.RoleId).NotEmpty()
            .MustAsync((roleId, cancellationToken) => dbContext.Roles.AnyAsync(r => r.Id == roleId, cancellationToken))
            .WithMessage("Invalid roleId.");

        RuleFor(request => request.FullName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.UserName).NotEmpty().MaximumLength(100);
    }
}
