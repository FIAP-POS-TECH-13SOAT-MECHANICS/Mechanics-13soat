using Mechanics.Application.Auth.Handlers;
using Mechanics.Application.Auth.Requests;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Users")]
public class GetUserHandlerTests
{
    [TestMethod("Retorna usuário quando o Id existe.")]
    public async Task It_ShouldReturnUser_WhenIdExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = UserMocks.CreateUser(userId, RoleMocks.CreateMechanicRole(roleId));

        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Users.Add(user))
            .Build();

        var handler = new GetUserHandler(context);
        var request = new GetUserRequest { Id = userId };

        // Act
        var response = await handler.Handle(request, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreEqual(userId, response.Id);
        Assert.AreEqual(user.FullName, response.FullName);
        Assert.AreEqual(user.UserName, response.UserName);
        Assert.IsNotNull(response.Role);
        Assert.AreEqual(roleId, response.Role.Id);
        Assert.AreEqual(RoleNames.Mechanic, response.Role.Name);
    }

    [TestMethod("Retorna null quando o Id não existe.")]
    public async Task It_ShouldReturnNull_WhenIdDoesNotExist()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new GetUserHandler(context);
        var request = new GetUserRequest { Id = Guid.NewGuid() };

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsNull(response);
    }

    public TestContext TestContext { get; set; }
}
