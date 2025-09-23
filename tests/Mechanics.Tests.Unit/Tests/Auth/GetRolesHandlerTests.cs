using AutoMapper;
using Mechanics.Application.Auth.Handlers;
using Mechanics.Application.Auth.Requests;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;

namespace Mechanics.Tests.Unit.Tests.Auth;

[TestClass]
[TestCategory("Auth")]
[TestCategory("Roles")]
public class GetRolesHandlerTests
{
    private readonly IMapper _createMap = AutoMapperFactory.CreateMap("Auth");

    [TestMethod("Contém todas as roles.")]
    public async Task It_ShouldReturnList_WithSameCountAsRoleNames()
    {
        // Arrange
        var roleNames = RoleMocks.GetRoleNames();
        await using var context = new DbContextTestBuilder()
            .WithData(roleNames)
            .Build();
        var handler = new GetRolesHandler(context, _createMap);

        // Act
        var response = await handler.Handle(new GetRolesRequest(), TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.Items);
        Assert.AreEqual(roleNames.Count, response.Items.Count());
    }

    public TestContext TestContext { get; set; }
}
