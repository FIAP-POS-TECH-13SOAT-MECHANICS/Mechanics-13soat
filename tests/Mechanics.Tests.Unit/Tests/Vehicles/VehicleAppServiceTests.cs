using AutoMapper;
using Mechanics.Application.Utils.CommonResponses;
using Mechanics.Application.Vehicles.Requests;
using Mechanics.Application.Vehicles.Services;
using Mechanics.Domain.Base.Validation;
using Mechanics.Domain.Customers;
using Mechanics.Infra.Data;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Mechanics.Tests.Unit.Tests.Vehicles;

[TestClass]
[TestCategory("Vehicles")]
public class VehicleAppServiceTests
{
    public TestContext TestContext { get; set; }
    private readonly IMapper _mapper = AutoMapperFactory.CreateMap("Vehicles");

    private static (Guid owner1, Guid owner2, Customer c1, Customer c2) SeedOwners(AppDbContext context)
    {
        var owner1 = Guid.NewGuid();
        var owner2 = Guid.NewGuid();
        var c1 = new Customer { Id = owner1, Name = "Owner One", Email = "owner1@example.com", Document = new PersonalDocument(DocumentType.Cpf, "52998224725") };
        var c2 = new Customer { Id = owner2, Name = "Owner Two", Email = "owner2@example.com", Document = new PersonalDocument(DocumentType.Cpf, "11144477735") };
        context.Customers.AddRange(c1, c2);
        context.SaveChanges();
        return (owner1, owner2, c1, c2);
    }

    [TestMethod("Cadastro de veículo")]
    public async Task It_ShouldCreateVehicle()
    {
        await using var context = new DbContextTestBuilder().Build();
        var (ownerId, _, _, _) = SeedOwners(context);
        var handler = new VehicleAppService(context, _mapper);
        var request = VehicleMocks.BuildCreateRequest(ownerId);

        var response = await handler.Create(request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.AreNotEqual(Guid.Empty, response.CreatedId);
        var created = await context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == response.CreatedId, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(created);
        Assert.AreEqual(request.Manufacturer.Trim().ToUpper(), created.Manufacturer);
        Assert.AreEqual(request.Model.Trim().ToUpper(), created.Model);
        Assert.AreEqual(request.Color!.Value, created.Color);
        Assert.AreEqual("2020", created.Year);
        Assert.AreEqual(request.LicensePlate, created.LicensePlate.Number);
        Assert.AreEqual(request.Chassis, created.Chassis);
        Assert.AreEqual(request.OwnerId, created.OwnerId);
    }

    [TestMethod("Cadastro de veículo com placa inválida")]
    public async Task It_ShouldThrow_WhenLicensePlateIsInvalid()
    {
        await using var context = new DbContextTestBuilder().Build();
        var (ownerId, _, _, _) = SeedOwners(context);
        var handler = new VehicleAppService(context, _mapper);
        var request = VehicleMocks.BuildInvalidPlateCreateRequest(ownerId);

        await Assert.ThrowsExactlyAsync<DomainValidationException>(async () =>
        {
            await handler.Create(request, CancellationToken.None);
        });
    }

    [TestMethod("Alteração de veículo")]
    public async Task It_ShouldUpdateVehicle()
    {
        var id = Guid.NewGuid();
        await using var context = new DbContextTestBuilder().Build();
        var (owner1, owner2, _, _) = SeedOwners(context);
        var existing = VehicleMocks.CreateVehicle(id, owner1);
        context.Vehicles.Add(existing);
        await context.SaveChangesAsync();

        var handler = new VehicleAppService(context, _mapper);
        var request = VehicleMocks.BuildUpdateRequest(owner2);

        var response = await handler.Update(id, request, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.IsInstanceOfType<UpdateItemResponse>(response);
        var updated = await context.Vehicles.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, TestContext.CancellationTokenSource.Token);
        Assert.IsNotNull(updated);
        Assert.AreEqual(request.Manufacturer!.Trim().ToUpper(), updated.Manufacturer);
        Assert.AreEqual(request.Model!.Trim().ToUpper(), updated.Model);
        Assert.AreEqual(request.Color!.Value, updated.Color);
        Assert.AreEqual("2021", updated.Year);
        Assert.AreEqual(request.LicensePlate, updated.LicensePlate.Number);
        Assert.AreEqual(request.Chassis, updated.Chassis);
        Assert.AreEqual(owner2, updated.OwnerId);
    }

    [TestMethod("Consulta por ID")]
    public async Task It_ShouldGetVehicleById()
    {
        var id = Guid.NewGuid();
        await using var context = new DbContextTestBuilder().Build();
        var (ownerId, _, _, _) = SeedOwners(context);
        var existing = VehicleMocks.CreateVehicle(id, ownerId, plate: "DEF2G34", chassis: "9BWZZZ377VT004252");
        context.Vehicles.Add(existing);
        await context.SaveChangesAsync();

        var handler = new VehicleAppService(context, _mapper);

        var response = await handler.Get(id, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(response);
        Assert.AreEqual(id, response!.Id);
        Assert.AreEqual(existing.Manufacturer, response.Manufacturer);
        Assert.AreEqual(existing.Model, response.Model);
        Assert.AreEqual(existing.Color, response.Color);
        Assert.AreEqual(existing.Year, response.Year);
        Assert.AreEqual(existing.LicensePlate.ToString(), response.LicensePlate);
        Assert.AreEqual(existing.Chassis, response.Chassis);
        Assert.AreEqual(existing.OwnerId, response.OwnerId);
    }

    [TestMethod("Listar por proprietário (3 registros, sendo dois do mesmo)")]
    public async Task It_ShouldListByOwner()
    {
        await using var context = new DbContextTestBuilder().Build();
        var (owner1, owner2, _, _) = SeedOwners(context);

        var v1 = VehicleMocks.CreateVehicle(Guid.NewGuid(), owner1, plate: "AAA1A11", chassis: "9BWZZZ377VT004261");
        var v2 = VehicleMocks.CreateVehicle(Guid.NewGuid(), owner1, plate: "BBB2B22", chassis: "9BWZZZ377VT004262");
        var v3 = VehicleMocks.CreateVehicle(Guid.NewGuid(), owner2, plate: "CCC3C33", chassis: "9BWZZZ377VT004263");
        context.Vehicles.AddRange(v1, v2, v3);
        await context.SaveChangesAsync();

        var handler = new VehicleAppService(context, _mapper);
        var request = new GetVehiclesRequest { OwnerId = owner1, Page = 1, ItemsPerPage = 10 };

        var list = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(list);
        Assert.AreEqual(2, list.Items.Count());
        Assert.AreEqual(2, list.TotalCount);
    }

    [TestMethod("Listar por placa")]
    public async Task It_ShouldListByLicensePlate()
    {
        await using var context = new DbContextTestBuilder().Build();
        var (ownerId, _, _, _) = SeedOwners(context);
        var targetPlate = "JKA5K67";
        var v1 = VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: targetPlate, chassis: "9BWZZZ377VT004271");
        var v2 = VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "XYZ1Z23", chassis: "9BWZZZ377VT004272");
        context.Vehicles.AddRange(v1, v2);
        await context.SaveChangesAsync();

        var handler = new VehicleAppService(context, _mapper);
        var request = new GetVehiclesRequest { LicensePlate = targetPlate, Page = 1, ItemsPerPage = 10 };

        var list = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Items.Count());
        Assert.AreEqual(1, list.TotalCount);
        Assert.AreEqual(targetPlate, list.Items.First().LicensePlate);
    }

    [TestMethod("Listar por chassi")]
    public async Task It_ShouldListByChassis()
    {
        await using var context = new DbContextTestBuilder().Build();
        var (ownerId, _, _, _) = SeedOwners(context);
        var targetChassis = "9BWZZZ377VT004281";
        var v1 = VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "JKA5K67", chassis: targetChassis);
        var v2 = VehicleMocks.CreateVehicle(Guid.NewGuid(), ownerId, plate: "XYZ1Z23", chassis: "9BWZZZ377VT004272");
        context.Vehicles.AddRange(v1, v2);
        await context.SaveChangesAsync();

        var handler = new VehicleAppService(context, _mapper);
        var request = new GetVehiclesRequest { Chassis = targetChassis, Page = 1, ItemsPerPage = 10 };

        var list = await handler.GetList(request, TestContext.CancellationTokenSource.Token);

        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Items.Count());
        Assert.AreEqual(1, list.TotalCount);
        Assert.AreEqual(targetChassis, list.Items.First().Chassis);
    }
}
