using AutoMapper;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Customers;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mechanics.Tests.Unit.Tests.WorkOrders;

[TestClass]
[TestCategory("WorkOrder")]
public class WorkOrderAppServiceTests
{
    public TestContext TestContext { get; set; } = null!;

    private IMapper _mapper = null!;
    private EmailServiceMock _emailMock = null!;
    private NullLoggerFactory _loggerFactory = null!;

    [TestInitialize]
    public void Initialize()
    {
        try
        {
            _mapper = Mechanics.Tests.Unit.Helpers.AutoMapperFactory.CreateMap("WorkOrders");
        }
        catch
        {
            var cfg = new MapperConfiguration(c => { try { c.AddProfile(new Mechanics.Application.WorkOrders.WorkOrderMapperProfile()); } catch { } }, new NullLoggerFactory());
            _mapper = cfg.CreateMapper();
        }

        _emailMock = new EmailServiceMock();
        _loggerFactory = new NullLoggerFactory();
    }

    [TestMethod("Create should persist work order and send created email")]
    public async Task Create_ShouldPersistAndSendEmail()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = customerId, Name = "John", Email = "john@example.com", Document = new Mechanics.Domain.Customers.PersonalDocument(Mechanics.Domain.Customers.DocumentType.Cpf, "12345678909") });
                ctx.Vehicles.Add(new Vehicle { Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = Mechanics.Domain.Vehicles.VehicleColor.White, Year = "2020", LicensePlate = new Mechanics.Domain.Vehicles.LicensePlate("ABC1234"), Chassis = "CH", OwnerId = customerId });
            })
            .Build();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var request = new CreateWorkOrderRequest
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            ProductIds = Array.Empty<Guid>(),
            ServiceCatalogIds = Array.Empty<Guid>(),
            ReportedProblem = "Test problem"
        };

        var id = await service.Create(request, TestContext.CancellationTokenSource.Token);

        var wo = await context.WorkOrders.FindAsync(id);
        Assert.IsNotNull(wo, "Work order should be persisted");
        Assert.AreEqual(customerId, wo!.CustomerId, "CustomerId persisted");
        Assert.IsTrue(_emailMock.SendWorkOrderCreatedCalled, "SendWorkOrderCreated should be called");
    }

    [TestMethod("RequestApproval should calculate estimate, set PendingApproval and send email")]
    public async Task RequestApproval_ShouldCalculateEstimateAndSendEmail()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var svcId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = customerId, Name = "Mary", Email = "mary@example.com", Document = new Mechanics.Domain.Customers.PersonalDocument(Mechanics.Domain.Customers.DocumentType.Cpf, "98765432100") });
                ctx.Vehicles.Add(new Vehicle { Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = Mechanics.Domain.Vehicles.VehicleColor.Black, Year = "2021", LicensePlate = new Mechanics.Domain.Vehicles.LicensePlate("DEF5678"), Chassis = "CH2", OwnerId = customerId });
                ctx.ServiceCatalog.Add(new ServiceCatalog { Id = svcId, Name = "Oil change", Description = "Change oil", BasePrice = 100m, AverageTime = 30, Status = Mechanics.Domain.Base.ServiceCatalogStatusType.Active });
            })
            .Build();

        var svc = await context.ServiceCatalog.FindAsync(svcId);
        var wo = WorkOrderMocks.CreateWorkOrderWithServices(Guid.NewGuid(), customerId, vehicleId, svc!);

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        var performedBy = Guid.NewGuid();
        await service.RequestApproval(wo.Id, performedBy, TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders.FindAsync(wo.Id);
        Assert.IsNotNull(reloaded);
        Assert.AreEqual(WorkOrderStatus.PendingApproval, reloaded!.Status);
        Assert.IsNotNull(reloaded.ApprovalRequestedAt);
        Assert.AreEqual(performedBy, reloaded.LastStatusChangeBy);
        Assert.IsTrue(_emailMock.SendWorkOrderPendingApprovalCalled);

        Assert.AreEqual(100m, _emailMock.LastBudgetTotal);
    }

    [TestMethod("ChangeStatus should require approval before InProgress and should record history")]
    public async Task ChangeStatus_ShouldValidateAndRecordHistory()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer { Id = customerId, Name = "Pedro", Email = "pedro@example.com", Document = new Mechanics.Domain.Customers.PersonalDocument(Mechanics.Domain.Customers.DocumentType.Cpf, "11122233344") });
                ctx.Vehicles.Add(new Vehicle { Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = Mechanics.Domain.Vehicles.VehicleColor.Gray, Year = "2019", LicensePlate = new Mechanics.Domain.Vehicles.LicensePlate("GHI9012"), Chassis = "CH3", OwnerId = customerId });
            })
            .Build();

        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerId, vehicleId);
        wo.Status = WorkOrderStatus.PendingApproval;
        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync();

        var budgetService = new BudgetAppService(
            context,
            _emailMock,
            _loggerFactory.CreateLogger<BudgetAppService>());

        var service = new WorkOrderAppService(
            context,
            _mapper,
            _emailMock,
            _loggerFactory.CreateLogger<WorkOrderAppService>(),
            budgetService);

        await Assert.ThrowsExactlyAsync<BusinessException>(() => service.ChangeStatus(wo.Id, WorkOrderStatus.InProgress, Guid.NewGuid(), TestContext.CancellationTokenSource.Token));

        wo.ApprovedAt = DateTime.Now;
        context.WorkOrders.Update(wo);
        await context.SaveChangesAsync();

        var statusChangedBy = Guid.NewGuid();
        await service.ChangeStatus(wo.Id, WorkOrderStatus.InProgress, statusChangedBy, TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders.FindAsync(wo.Id);
        Assert.IsNotNull(reloaded);
        Assert.AreEqual(WorkOrderStatus.InProgress, reloaded!.Status);
        Assert.AreEqual(statusChangedBy, reloaded.LastStatusChangeBy);
        Assert.IsTrue(context.WorkOrderHistories.Any(h => h.WorkOrderId == wo.Id && h.Action == "StatusChanged"));
        Assert.IsTrue(_emailMock.SendWorkOrderStatusChangedCalled);
    }
}
