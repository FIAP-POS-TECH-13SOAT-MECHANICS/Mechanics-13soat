using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;
using Microsoft.AspNetCore.Identity;

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
            SecurityStamp = userId.ToString(),
        };
    }

    public static User CreateUser(string userName, string userPassword)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = userName.ToUpper(),
            UserName = userName,
            RoleId = Guid.Empty,
            Role = new Role { Name = RoleNames.Administrator },
            Email = $"{userName}@mechanics.com",
            PasswordHash = "",
            SecurityStamp = userName,
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, userPassword);

        return user;
    }
}
