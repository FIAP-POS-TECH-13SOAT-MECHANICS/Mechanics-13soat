using Mechanics.Application.Auth.Handlers;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Users")]
public class CreateUserHandlerTests
{
    [TestMethod("Cria usuário quando RoleId é válido.")]
    public async Task It_ShouldCreateUser_WhenRoleIdIsValid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Roles.Add(new Role { Id = roleId, Name = RoleNames.Administrator }))
            .Build();
        var handler = new CreateUserHandler(context);
        var request = UserMocks.BuildRequest(roleId);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == response.CreatedId);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.FullName, created.FullName);
        Assert.AreEqual(request.UserName, created.UserName);
        Assert.AreEqual(request.RoleId, created.RoleId);
    }

    [TestMethod("Falha quando RoleId é inválido.")]
    public async Task It_ShouldThrow_WhenRoleIdIsInvalid()
    {
        await using var context = new DbContextTestBuilder().Build();
        var handler = new CreateUserHandler(context);
        var request = UserMocks.BuildRequest(new Guid("f2d59afa-6e85-4557-8ff1-733343ba83f8"));

        var ex = await Assert.ThrowsExactlyAsync<KeyNotFoundException>(() => handler.Handle(request, CancellationToken.None));
        Assert.AreEqual("Role not found", ex.Message);
    }
}
