using FluentValidation;
using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Base.Validation;
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

        RuleFor(request => request.CpfNumber).NotEmpty()
            .Must(DocumentValidations.ValidateCpf)
            .WithMessage("Invalid CPF.");

        RuleFor(request => request.CpfNumber)
            .MustAsync((cpf, cancellationToken) => dbContext.Users.AllAsync(u => u.CpfNumber != cpf, cancellationToken))
            .WithMessage("This CPF is already registered.");

        RuleFor(request => request.Email).NotEmpty().EmailAddress()
            .MustAsync((email, cancellationToken) => dbContext.Users.AllAsync(u => u.Email != email, cancellationToken))
            .WithMessage("This email is already registered.");
    }
}
