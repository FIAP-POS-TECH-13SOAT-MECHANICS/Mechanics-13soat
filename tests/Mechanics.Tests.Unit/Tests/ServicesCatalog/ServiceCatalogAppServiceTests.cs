using AutoMapper;
using Mechanics.Application.ServicesCatalog.Services;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Domain.Base.Validation;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.ServicesCatalog;

[TestClass]
[TestCategory("ServicesCatalog")]
public class ServicesCatalogAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("ServicesCatalog");

    #region cadastrar serviço

    [TestMethod("Cria serviço")]
    public async Task It_ShouldCreateService()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildCreateRequest();

        // Act
        var response = await handler.Create(request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.ServiceCatalog.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Name.Trim().ToUpper(), created.Name);
        Assert.AreEqual(request.Description.Trim(), created.Description);
        Assert.AreEqual(request.BasePrice, created.BasePrice);
        Assert.AreEqual(request.AverageTime, created.AverageTime);
        Assert.AreEqual(request.Status ?? created.Status, created.Status);
    }

    [TestMethod("Falha ao criar serviço inválido")]
    public async Task It_ShouldThrow_WhenCreateServiceIsInvalid()
    {
        // Arrange
        await using var context = new DbContextTestBuilder().Build();
        var handler = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildInvalidCreateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.Create(request, CancellationToken.None);
        });
    }

    #endregion

    #region atualizar serviço

    [TestMethod("Atualiza dados de um serviço cadastrado")]
    public async Task It_ShouldUpdateService_WhenDataIsValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ServicesCatalogMocks.CreateService(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.ServiceCatalog.Add(existing)).Build();
        var handler = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildUpdateRequest();

        // Act
        var response = await handler.Update(id, request, CancellationToken.None);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);
        var updated = await context.ServiceCatalog.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(request.Name!.Trim().ToUpper(), updated.Name);
        Assert.AreEqual(request.Description, updated.Description);
        Assert.AreEqual(request.BasePrice, updated.BasePrice);
        Assert.AreEqual(request.AverageTime, updated.AverageTime);
        Assert.AreEqual(request.Status, updated.Status);
    }

    [TestMethod("Falha ao atualizar serviço inválido")]
    public async Task It_ShouldThrow_WhenUpdateServiceIsInvalid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = ServicesCatalogMocks.CreateInvalidService(id);
        await using var context = new DbContextTestBuilder().WithData(ctx => ctx.ServiceCatalog.Add(existing)).Build();
        var handler = new ServiceCatalogAppService(context, _mapper);
        var request = ServicesCatalogMocks.BuildInvalidUpdateRequest();

        // Act + Assert
        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.Update(id, request, CancellationToken.None);
        });
    }

    #endregion
}
