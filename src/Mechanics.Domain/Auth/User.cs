using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;

namespace Mechanics.Domain.Auth;

public class User : AbstractEntity, INormalizable, IValidatable
{
    public required string FullName { get; set; }
    public Role? Role { get; init; }
    public required Guid RoleId { get; set; }
    public required string UserName { get; set; }

    public bool IsNormalized() =>
        FullName.IsTrimmedUpperCase() &&
        UserName.IsTrimmedLowerCase();

    public void Normalize()
    {
        FullName = FullName.ToUpper().Trim();
        UserName = UserName.ToLower().Trim();
    }

    public void Validate(ValidationBuilder builder)
    {
        builder.AddValidation(FullName.Length > 0, nameof(FullName), "Full name is required.")
            .AddValidation(UserName.Length > 0, nameof(UserName), "User name is required.");
    }
}
