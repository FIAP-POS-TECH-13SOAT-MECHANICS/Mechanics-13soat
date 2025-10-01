using Mechanics.Domain.Base;

namespace Mechanics.Domain.Auth;

public class User : AbstractEntity
{
    public required string FullName { get; set; }
    public Role? Role { get; init; }
    public required Guid RoleId { get; set; }
    public required string UserName { get; set; }
}
