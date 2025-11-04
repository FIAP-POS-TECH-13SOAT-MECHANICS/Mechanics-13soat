using AutoMapper;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Services;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base;
using Mechanics.Domain.Base.Exceptions;
using Mechanics.Domain.Customers;
using Mechanics.Domain.Products;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Tests.Unit.Helpers;
using Mechanics.Tests.Unit.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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
        _mapper = AutoMapperFactory.CreateMap("WorkOrders");

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
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "John", Email = "john@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12345678909")
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = VehicleColor.White, Year = "2020",
                    LicensePlate = new LicensePlate("ABC1234"), Chassis = "CH", OwnerId = customerId
                });
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
            VehicleId = vehicleId,
            ProductIds = [],
            ServiceCatalogIds = [],
            ReportedProblem = "Test problem",
        };

        var id = await service.Create(request, TestContext.CancellationTokenSource.Token);

        var wo = await context.WorkOrders.FindAsync([id], TestContext.CancellationTokenSource.Token);
        IsNotNull(wo, "Work order should be persisted");
        AreEqual(customerId, wo.CustomerId, "CustomerId persisted");
        IsTrue(_emailMock.SendWorkOrderCreatedCalled, "SendWorkOrderCreated should be called");
    }

    [TestMethod("Create should throw when vehicle owner customer does not exist")]
    public async Task Create_ShouldThrow_WhenVehicleOwnerCustomerMissing()
    {
        var orphanOwnerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2022",
                    LicensePlate = new LicensePlate("ORP1234"),
                    Chassis = "CHORP",
                    OwnerId = orphanOwnerId,
                });
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
            VehicleId = vehicleId,
        };

        await ThrowsExactlyAsync<EntityNotFoundException>(() =>
            service.Create(request, TestContext.CancellationTokenSource.Token));
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
                ctx.Customers.Add(new Customer
                {
                    Id = customerId, Name = "Mary", Email = "mary@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "98765432100")
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId, Manufacturer = "Make", Model = "Model", Color = VehicleColor.Black, Year = "2021",
                    LicensePlate = new LicensePlate("DEF5678"), Chassis = "CH2", OwnerId = customerId
                });
                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = svcId, Name = "Oil change", Description = "Change oil", BasePrice = 100m, AverageTime = 30,
                    Status = ServiceCatalogStatusType.Active
                });
            })
            .Build();

        var svc = await context.ServiceCatalog.FindAsync([svcId], TestContext.CancellationTokenSource.Token);
        var wo = WorkOrderMocks.CreateWorkOrderWithServices(Guid.NewGuid(), customerId, vehicleId, svc!);

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

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

        var reloaded = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(WorkOrderStatus.PendingApproval, reloaded.Status);
        IsNotNull(reloaded.ApprovalRequestedAt);
        AreEqual(performedBy, reloaded.LastStatusChangeBy);
        IsTrue(_emailMock.SendWorkOrderPendingApprovalCalled);

        AreEqual(100m, _emailMock.LastBudgetTotal);
    }

    [TestMethod("ChangeStatus should require approval before InProgress and should record history")]
    public async Task ChangeStatus_ShouldValidateAndRecordHistory()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Pedro",
                    Email = "pedro@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "11122233344")
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.Gray,
                    Year = "2019",
                    LicensePlate = new LicensePlate("GHI9012"),
                    Chassis = "CH3",
                    OwnerId = customerId
                });
            })
            .Build();

        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerId, vehicleId);
        wo.Status = WorkOrderStatus.PendingApproval;
        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

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

        var mechanicRoleId = Guid.NewGuid();
        var mechanicRole = new Role
        {
            Id = mechanicRoleId,
            Name = RoleNames.Mechanic,
            CreationDate = DateTime.UtcNow
        };
        context.Roles.Add(mechanicRole);

        var performingUserId = Guid.NewGuid();
        var performingUser = new User
        {
            Id = performingUserId,
            FullName = "Test Mechanic",
            UserName = "test_mechanic",
            Email = "test_mechanic@example.com",
            PasswordHash = "hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = mechanicRoleId,
            CreationDate = DateTime.UtcNow
        };
        context.Users.Add(performingUser);

        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await ThrowsExactlyAsync<BusinessException>(() =>
            service.ChangeStatus(wo.Id, WorkOrderStatus.Received, performingUserId, comment: null,
                TestContext.CancellationTokenSource.Token));

        wo.ApprovedAt = DateTime.Now;
        context.WorkOrders.Update(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var statusChangedBy = Guid.NewGuid();
        var otherUser = new User
        {
            Id = statusChangedBy,
            FullName = "Another Mechanic",
            UserName = "another_mechanic",
            Email = "another_mechanic@example.com",
            PasswordHash = "hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = mechanicRoleId,
            CreationDate = DateTime.UtcNow
        };
        context.Users.Add(otherUser);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await service.ChangeStatus(wo.Id, WorkOrderStatus.InProgress, statusChangedBy, comment: null,
            TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(WorkOrderStatus.InProgress, reloaded.Status);
        AreEqual(statusChangedBy, reloaded.LastStatusChangeBy);
        var histories = await context.WorkOrderHistories.Where(h => h.WorkOrderId == wo.Id && h.Action == "StatusChanged")
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        IsNotEmpty(histories);
        IsTrue(_emailMock.SendWorkOrderStatusChangedCalled);
    }

    [TestMethod("ChangeStatus should reject same status and skip notifications")]
    public async Task ChangeStatus_ShouldRejectSameStatus()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Laura",
                    Email = "laura@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "55566677788")
                });
                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.Red,
                    Year = "2018",
                    LicensePlate = new LicensePlate("JKL3456"),
                    Chassis = "CH4",
                    OwnerId = customerId
                });
            })
            .Build();

        var wo = WorkOrderMocks.CreateWorkOrderEntity(Guid.NewGuid(), customerId, vehicleId);
        AreEqual(WorkOrderStatus.Received, wo.Status);

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

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

        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = RoleNames.Mechanic,
            CreationDate = DateTime.UtcNow
        };
        context.Roles.Add(role);

        var actorId = Guid.NewGuid();
        var actor = new User
        {
            Id = actorId,
            FullName = "Actor User",
            UserName = "actor_user",
            Email = "actor@example.com",
            PasswordHash = "hash",
            SecurityStamp = Guid.NewGuid().ToString(),
            RoleId = roleId,
            CreationDate = DateTime.UtcNow
        };
        context.Users.Add(actor);

        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        await ThrowsExactlyAsync<BusinessException>(() =>
            service.ChangeStatus(wo.Id, WorkOrderStatus.Received, actorId, comment: null,
                TestContext.CancellationTokenSource.Token));

        IsFalse(_emailMock.SendWorkOrderStatusChangedCalled, "Status change email should not be sent");
        var histories = await context.WorkOrderHistories.Where(h => h.WorkOrderId == wo.Id)
            .ToListAsync(TestContext.CancellationTokenSource.Token);
        IsEmpty(histories, "No history should be recorded");

        var reloaded = await context.WorkOrders.FindAsync([wo.Id], TestContext.CancellationTokenSource.Token);
        IsNotNull(reloaded);
        AreEqual(WorkOrderStatus.Received, reloaded.Status);
    }

    [TestClass]
    [TestCategory("WorkOrder")]
    public class WorkOrderAppServiceTransitionsTests
    {
        [TestMethod("IsTransitionAllowed deve permitir o fluxo principal e rejeitar transições inválidas ou iguais")]
        public void IsTransitionAllowed_ValidAndInvalidTransitions()
        {
            IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.Received, WorkOrderStatus.UnderDiagnosis));
            IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.UnderDiagnosis, WorkOrderStatus.PendingApproval));
            IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.PendingApproval, WorkOrderStatus.InProgress));
            IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.InProgress, WorkOrderStatus.Completed));
            IsTrue(InvokeIsTransitionAllowed(WorkOrderStatus.Completed, WorkOrderStatus.Delivered));

            // transição inválida (pular etapas)
            IsFalse(InvokeIsTransitionAllowed(WorkOrderStatus.Received, WorkOrderStatus.InProgress));

            // transição para o mesmo status deve ser considerada inválida no método
            IsFalse(InvokeIsTransitionAllowed(WorkOrderStatus.Received, WorkOrderStatus.Received));
            IsFalse(InvokeIsTransitionAllowed(WorkOrderStatus.Completed, WorkOrderStatus.Completed));
        }

        private static bool InvokeIsTransitionAllowed(WorkOrderStatus from, WorkOrderStatus to)
        {
            var method = typeof(WorkOrderAppService).GetMethod("IsTransitionAllowed", BindingFlags.NonPublic | BindingFlags.Static);
            IsNotNull(method, "Método IsTransitionAllowed não encontrado. Verifique a assinatura e a visibilidade.");
            return (bool)method.Invoke(null, [from, to])!;
        }
    }

    [TestMethod("UpdateDetails should add products, services and observations and record history")]
    public async Task UpdateDetails_ShouldAddProductsServicesAndObservations()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var mechanicUserId = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Client",
                    Email = "client@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12345678909")
                });

                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2020",
                    LicensePlate = new LicensePlate("ABC1234"),
                    Chassis = "CH",
                    OwnerId = customerId
                });

                ctx.Products.Add(new Product
                {
                    Id = productId,
                    Name = "Filtro",
                    Description = "Filtro de óleo",
                    Quantity = 5,
                    Status = ProductStatusType.Active,
                    Type = ProductType.Part
                });

                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = serviceId,
                    Name = "Troca de Filtro",
                    Description = "Troca de filtro",
                    BasePrice = 50m,
                    AverageTime = 20,
                    Status = ServiceCatalogStatusType.Active
                });
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

        // create base work order without products/services
        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now
        };
        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

        var req = new UpdateWorkOrderRequest
        {
            ProductIds = [productId],
            ServiceIds = [serviceId],
            Observations = "Substituir filtro e testar motor"
        };

        await service.UpdateDetails(wo.Id, req, mechanicUserId, TestContext.CancellationTokenSource.Token);

        var reloaded = await context.WorkOrders
            .Include(w => w.Products)
            .Include(w => w.ServiceCatalog)
            .FirstOrDefaultAsync(w => w.Id == wo.Id, TestContext.CancellationTokenSource.Token);

        IsNotNull(reloaded);
        IsTrue(reloaded.Products != null && reloaded.Products.Any(p => p.Id == productId));
        IsTrue(reloaded.ServiceCatalog != null && reloaded.ServiceCatalog.Any(s => s.Id == serviceId));
        AreEqual(req.Observations, reloaded.Observations);

        var history = await context.WorkOrderHistories.Where(h => h.WorkOrderId == wo.Id && h.Action == "DetailsUpdated")
            .FirstOrDefaultAsync(TestContext.CancellationTokenSource.Token);
        IsNotNull(history);
        IsNotNull(history.Details);
        AreEqual(mechanicUserId, history.PerformedByUserId);
        Contains("AddedProducts:1", history.Details);
        Contains("AddedServices:1", history.Details);
        Contains("ObservationsUpdated", history.Details);
    }

    // New tests for GetAverageServiceTime
    [TestMethod("GetAverageServiceTime should return total average time for associated services")]
    public async Task GetAverageServiceTime_ShouldReturnSumOfAverageTimes()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var svc1Id = Guid.NewGuid();
        var svc2Id = Guid.NewGuid();

        await using var context = new DbContextTestBuilder()
            .WithData(ctx =>
            {
                ctx.Customers.Add(new Customer
                {
                    Id = customerId,
                    Name = "Client",
                    Email = "client@example.com",
                    Document = new PersonalDocument(DocumentType.Cpf, "12312312312")
                });

                ctx.Vehicles.Add(new Vehicle
                {
                    Id = vehicleId,
                    Manufacturer = "Make",
                    Model = "Model",
                    Color = VehicleColor.White,
                    Year = "2020",
                    LicensePlate = new LicensePlate("AVG1234"),
                    Chassis = "CHAVG",
                    OwnerId = customerId
                });

                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = svc1Id,
                    Name = "Service 1",
                    Description = "S1",
                    BasePrice = 10m,
                    AverageTime = 30,
                    Status = ServiceCatalogStatusType.Active
                });

                ctx.ServiceCatalog.Add(new ServiceCatalog
                {
                    Id = svc2Id,
                    Name = "Service 2",
                    Description = "S2",
                    BasePrice = 20m,
                    AverageTime = 45,
                    Status = ServiceCatalogStatusType.Active
                });
            })
            .Build();

        var svc1 = await context.ServiceCatalog.FindAsync([svc1Id], TestContext.CancellationTokenSource.Token);
        var svc2 = await context.ServiceCatalog.FindAsync([svc2Id], TestContext.CancellationTokenSource.Token);

        var wo = new WorkOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            AccessKey = WorkOrder.GenerateNewAccessKey([]),
            Status = WorkOrderStatus.Received,
            CreationDate = DateTime.Now,
            LastUpdate = DateTime.Now,
            ServiceCatalog = new List<ServiceCatalog> { svc1!, svc2! }
        };

        context.WorkOrders.Add(wo);
        await context.SaveChangesAsync(TestContext.CancellationTokenSource.Token);

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

        var response = await service.GetAverageServiceTime(wo.Id, TestContext.CancellationTokenSource.Token);

        IsNotNull(response);
        AreEqual(wo.Id, response.WorkOrderId);
        AreEqual(30 + 45, response.TotalAverageTime);
    }

    [TestMethod("GetAverageServiceTime should return zero when work order not found")]
    public async Task GetAverageServiceTime_ShouldReturnZeroWhenWorkOrderNotFound()
    {
        await using var context = new DbContextTestBuilder().Build();

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

        var id = Guid.NewGuid();
        var response = await service.GetAverageServiceTime(id, TestContext.CancellationTokenSource.Token);

        IsNotNull(response);
        AreEqual(id, response.WorkOrderId);
        AreEqual(0, response.TotalAverageTime);
    }
}
