using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mechanics.Application.WorkOrders.Requests;
using Mechanics.Infra.Data;
using Mechanics.Tests.Integration.Helpers;
using Mechanics.Domain.Auth;
using Mechanics.Domain.Vehicles;
using Mechanics.Domain.Customers;
using Mechanics.Domain.ServicesCatalog;
using Mechanics.Domain.Base;

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
                Document = new PersonalDocument(DocumentType.Cpf, "12345678909")
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
                OwnerId = customerId
            });
            await db.SaveChangesAsync();
        }

        var createReq = new CreateWorkOrderRequest
        {
            CustomerId = customerId,
            VehicleId = vehicleId,
            ReportedProblem = "Teste integração"
        };

        var createResp = await client.PostAsJsonAsync("/api/work-orders", createReq);
        Assert.AreEqual(HttpStatusCode.Created, createResp.StatusCode);

        var createdBody = await createResp.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
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
                Status = ServiceCatalogStatusType.Active
            };

            db.ServiceCatalog.Add(svc);

            var wo = await db.WorkOrders.FindAsync(woId);
            if (wo is null)
                Assert.Fail("WorkOrder not found in DB after creation.");

            if (wo.ServiceCatalog is null)
                wo.ServiceCatalog = new List<ServiceCatalog>();
            ((List<ServiceCatalog>)wo.ServiceCatalog).Add(svc);

            await db.SaveChangesAsync();
        }

        var attendantUserId = new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7");

        var reqApprovalResp = await client.PostAsync($"/api/work-orders/{woId}/request-approval", null);
        Assert.AreEqual(HttpStatusCode.NoContent, reqApprovalResp.StatusCode);

        using (var scope = TestProperties.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var wo = await db.WorkOrders.FindAsync(woId);
            Assert.IsNotNull(wo);
            Assert.AreEqual(Mechanics.Domain.WorkOrders.WorkOrderStatus.PendingApproval, wo!.Status);
            Assert.IsNotNull(wo.ApprovalRequestedAt);
            Assert.AreEqual(attendantUserId, wo.LastStatusChangeBy);
        }
    }

    public TestContext TestContext { get; set; } = null!;
}
