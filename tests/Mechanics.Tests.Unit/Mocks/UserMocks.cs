using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;

namespace Mechanics.Tests.Unit.Mocks;

public static class UserMocks
{
    public static CreateUserRequest BuildRequest(Guid roleId) =>
        new()
        {
            FullName = "MARIA FERNANDA SOUZA",
            UserName = "maria.souza",
            RoleId = roleId,
        };

    public static User CreateUser(Guid userId, Role role)
    {
        return new User
        {
            Id = userId,
            FullName = "JOSÉ DA SILVA",
            UserName = "jose.silva",
            RoleId = role.Id,
            Role = role,
        };
    }
}
