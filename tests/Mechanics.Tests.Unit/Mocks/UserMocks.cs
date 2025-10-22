using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;

namespace Mechanics.Tests.Unit.Mocks;

public static class UserMocks
{
    public static CreateUserRequest BuildCreateRequest(Guid roleId) =>
        new()
        {
            FullName = "MARIA FERNANDA SOUZA",
            UserName = "maria.souza",
            RoleId = roleId,
            Email = "maria.souza@mechanics.com",
        };

    public static UpdateUserRequest BuildUpdateRequest(Guid roleId) =>
        new() { RoleId = roleId };

    public static User CreateUser(Guid userId, string name, Role role)
    {
        var userName = name.Replace(' ', '.').ToLower();

        return new User
        {
            Id = userId,
            FullName = name.ToUpper(),
            UserName = userName,
            RoleId = role.Id,
            Role = role,
            Email = $"{userName}@mechanics.com",
            PasswordHash = "",
            SecurityStamp = "",
        };
    }
}
