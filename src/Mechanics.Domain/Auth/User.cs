using Mechanics.Domain.Base;

namespace Mechanics.Domain.Auth;

public class User : AbstractEntity
{
    public required string FullName { get; init; }
    public Role? Role { get; init; }
    public required Guid RoleId { get; init; }
    public required string UserName { get; init; }
}
