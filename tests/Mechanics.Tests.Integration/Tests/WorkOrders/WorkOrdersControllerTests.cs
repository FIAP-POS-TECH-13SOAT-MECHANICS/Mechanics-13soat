using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Application.WorkOrders.Responses;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Base;
using Mechanics.Domain.Customers;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.WorkOrders;
using Mechanics.Infra.Data;
using Mechanics.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Tests.WorkOrders;

[TestClass]
public class WorkOrdersControllerTests
{
    [TestMethod]
    public async Task Create_And_RequestApproval_Flow_Works()
    {
        var client = await TestProperties.Factory.GetAuthenticatedClient(RoleNames.Attendant);

        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Customers.Add(new Customer
            {
                Id = customerId,
                Name = "Integration Customer",
                Email = "int.customer@example.com",
                Document = new PersonalDocument(DocumentType.Cpf, "12345678909"),
            });
            db.Vehicles.Add(new Vehicle
            {
                Id = vehicleId,
                Manufacturer = "Make",
                Model = "Model",
                Color = VehicleColor.Black,
                Year = "2020",
                LicensePlate = new LicensePlate("INT1234"),
                Chassis = "CHINT",
                OwnerId = customerId,
            });
            await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        }

        var createReq = new CreateWorkOrderRequest
        {
            VehicleId = vehicleId,
            ReportedProblem = "Teste integração",
        };

        var createResp = await client.PostAsJsonAsync("/api/work-orders", createReq, TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);

        var createdBody =
            await createResp.Content.ReadFromJsonAsync<Dictionary<string, Guid>>(TestContext.CancellationTokenSource.Token);
        if (createdBody is null)
        {
            Assert.Fail("Response body deserializado é nulo.");
            return;
        }

        Assert.IsTrue(createdBody.ContainsKey("id"));
        var woId = createdBody["id"];

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var svcId = Guid.NewGuid();
            var svc = new ServiceCatalog
            {
                Id = svcId,
                Name = "Teste Serviço",
                Description = "Serviço usado no teste de integração",
                BasePrice = 100m,
                AverageTime = 30,
                Status = ServiceCatalogStatusType.Active,
            };

            db.ServiceCatalog.Add(svc);

            var wo = await db.WorkOrders.Where(workOrder => workOrder.Id == woId)
                .Include(workOrder => workOrder.ServiceCatalog)
                .FirstOrDefaultAsync(TestContext.CancellationTokenSource.Token);
            if (wo is null)
                Assert.Fail("WorkOrder not found in DB after creation.");

            wo.ServiceCatalog ??= new List<ServiceCatalog>();
            wo.ServiceCatalog.Add(svc);

            await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        }

        var attendantUserId = new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7");

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Attendant,
                TestContext.CancellationTokenSource.Token);

            Guid roleId;
            if (role == null)
            {
                roleId = Guid.NewGuid();
                db.Roles.Add(new Role
                {
                    Id = roleId,
                    Name = RoleNames.Attendant,
                    CreationDate = DateTime.UtcNow
                });

                db.Users.Add(new User
                {
                    Id = attendantUserId,
                    FullName = "Integration Attendant",
                    UserName = "int_attendant",
                    Email = "int.attendant@example.com",
                    PasswordHash = "hash",
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RoleId = roleId,
                    CreationDate = DateTime.UtcNow
                });
            }
            else
            {
                roleId = role.Id;
                var existingUser = await db.Users.FindAsync([attendantUserId], TestContext.CancellationTokenSource.Token);
                if (existingUser == null)
                {
                    db.Users.Add(new User
                    {
                        Id = attendantUserId,
                        FullName = "Integration Attendant",
                        UserName = "int_attendant",
                        Email = "int.attendant@example.com",
                        PasswordHash = "hash",
                        SecurityStamp = Guid.NewGuid().ToString(),
                        RoleId = roleId,
                        CreationDate = DateTime.UtcNow
                    });
                }
            }

            await db.SaveChangesAsync(TestContext.CancellationTokenSource.Token);
        }

        var reqApprovalResp = await client.PostAsync($"/api/work-orders/{woId}/request-approval", null,
            TestContext.CancellationTokenSource.Token);
        Assert.AreEqual(HttpStatusCode.NoContent, reqApprovalResp.StatusCode);

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wo = await db.WorkOrders.FindAsync([woId], TestContext.CancellationTokenSource.Token);
            Assert.IsNotNull(wo);
            Assert.AreEqual(WorkOrderStatus.PendingApproval, wo.Status);
            Assert.IsNotNull(wo.ApprovalRequestedAt);
            Assert.AreEqual(attendantUserId, wo.LastStatusChangeBy);
        }
    }

    [TestMethod]
    public async Task It_ShouldReturnNotFound_WhenUserDocumentIsNotFound()
    {
        var client = await TestProperties.Factory.GetAuthenticatedClient(RoleNames.Attendant);

        var response = await client.GetAsync("/api/work-orders/track?document=12345678909&accessKey=123456",
            TestContext.CancellationTokenSource.Token);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }  

    public TestContext TestContext { get; set; } = null!;
}
