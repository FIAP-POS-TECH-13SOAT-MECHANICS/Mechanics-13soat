using AutoMapper;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Services;
using Mechanics.Domain.Auth;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Users")]
public class UserAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("Auth");

    #region cadastrar usuário

    [TestMethod("Cria usuário quando RoleId é válido.")]
    public async Task It_ShouldCreateUser_WhenRoleIdIsValid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        await using var context = new DbContextTestBuilder()
            .WithData(ctx => ctx.Roles.Add(new Role { Id = roleId, Name = RoleNames.Administrator }))
            .Build();
        var handler = new UserAppService(context, _mapper);
        var request = UserMocks.BuildRequest(roleId);

        // Act
        var response = await handler.CreateUser(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.FullName, created.FullName);
        Assert.AreEqual(request.UserName, created.UserName);
        Assert.AreEqual(request.RoleId, created.RoleId);
    }

    [TestMethod("Falha quando RoleId é inválido.")]
    public async Task It_ShouldThrow_WhenRoleIdIsInvalid()
    {
        await using var context = new DbContextTestBuilder().Build();
        var handler = new UserAppService(context, _mapper);
        var request = UserMocks.BuildRequest(new Guid("f2d59afa-6e85-4557-8ff1-733343ba83f8"));

        var ex = await Assert.ThrowsExactlyAsync<KeyNotFoundException>(() => handler
            .CreateUser(request, TestContext.CancellationTokenSource.Token));

        Assert.AreEqual("Role not found", ex.Message);
    }

    #endregion

    #region buscar usuário

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

        var handler = new UserAppService(context, _mapper);
        var request = new GetUserRequest { Id = userId };

        // Act
        var response = await handler.GetUser(request, TestContext.CancellationTokenSource.Token);

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
        var handler = new UserAppService(context, _mapper);
        var request = new GetUserRequest { Id = Guid.NewGuid() };

        // Act
        var response = await handler.GetUser(request, CancellationToken.None);

        // Assert
        Assert.IsNull(response);
    }

    #endregion
}
