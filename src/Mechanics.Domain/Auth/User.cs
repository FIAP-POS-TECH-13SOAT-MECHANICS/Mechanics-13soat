using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Validation;
using System.Security.Cryptography;
using System.Text;

namespace Mechanics.Domain.Auth;

public class User : AbstractEntity, INormalizable, IValidatable
{
    public required string FullName { get; set; }
    public Role? Role { get; init; }
    public required Guid RoleId { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string SecurityStamp { get; set; }

    public bool IsNormalized() =>
        FullName.IsTrimmedUpperCase() &&
        UserName.IsTrimmedLowerCase() &&
        Email.IsTrimmedLowerCase();

    public void Normalize()
    {
        FullName = FullName.ToUpperInvariant().Trim();
        UserName = UserName.ToLowerInvariant().Trim();
        Email = Email.ToLowerInvariant().Trim();
    }

    public void Validate(ValidationBuilder builder)
    {
        builder.AddValidation(FullName.Length > 0, nameof(FullName), "Full name is required.")
            .AddValidation(UserName.Length > 0, nameof(UserName), "User name is required.")
            .AddValidation(Email.Length > 0, nameof(Email), "Email is required.");
    }

    public string GetPasswordCreationCode()
    {
        byte[] userData =
        [
            ..Id.ToByteArray(),
            ..Encoding.ASCII.GetBytes(CreationDate.ToString("O")),
            ..Encoding.ASCII.GetBytes(SecurityStamp),
        ];
        var hashBytes = SHA256.HashData(userData);

        return Convert.ToHexString(hashBytes)[..32];
    }
}
