using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Mechanics");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Document = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCatalog",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageTime = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCatalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Manufacturer = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Year = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    LicensePlate = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false),
                    Chassis = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Customers_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "Mechanics",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SecurityStamp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Mechanics",
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessKey = table.Column<string>(type: "nchar(8)", fixedLength: true, maxLength: 8, nullable: false),
                    VehicleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    ReportedProblem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Observations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovalRequestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    LastStatusChangeBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Mechanics",
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrders_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalSchema: "Mechanics",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkOrders_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalSchema: "Mechanics",
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByCustomerDocument = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCatalogWorkOrder",
                schema: "Mechanics",
                columns: table => new
                {
                    ServiceCatalogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrdersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCatalogWorkOrder", x => new { x.ServiceCatalogId, x.WorkOrdersId });
                    table.ForeignKey(
                        name: "FK_ServiceCatalogWorkOrder_ServiceCatalog_ServiceCatalogId",
                        column: x => x.ServiceCatalogId,
                        principalSchema: "Mechanics",
                        principalTable: "ServiceCatalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceCatalogWorkOrder_WorkOrders_WorkOrdersId",
                        column: x => x.WorkOrdersId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderHistories",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderHistories_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderProduct",
                schema: "Mechanics",
                columns: table => new
                {
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderProduct", x => new { x.WorkOrderId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_WorkOrderProduct_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Mechanics",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderProduct_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BudgetItems",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    BudgetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ServiceCatalogId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NameSnapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UnitPriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetItems_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "Mechanics",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Mechanics",
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"), "ADMINISTRATOR" },
                    { new Guid("a1097867-aa3e-416c-8685-190516b62a12"), "ATTENDANT" },
                    { new Guid("f6027484-89a4-49f6-a9cb-4d1733c2bab7"), "MECHANIC" }
                });

            migrationBuilder.InsertData(
                schema: "Mechanics",
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Email", "FullName", "PasswordHash", "RoleId", "SecurityStamp", "UserName" },
                values: new object[,]
                {
                    { new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"), new DateTime(2025, 10, 12, 12, 0, 0, 0, DateTimeKind.Utc), "mechanic@mechanics.com", "Mechanic User", "AQAAAAIAAYagAAAAEKSeHdHtCfN38pakeil4oyEL0d07GBEySe6csY8jmXIKT3oEZVcZR7Jngd9qxFgmkQ==", new Guid("f6027484-89a4-49f6-a9cb-4d1733c2bab7"), "0a3bc211-1220-4d20-80e9-bd850d0dc200", "mechanic" },
                    { new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"), new DateTime(2025, 10, 12, 12, 0, 0, 0, DateTimeKind.Utc), "attendant@mechanics.com", "Attendant User", "AQAAAAIAAYagAAAAEEo/VptbCYVPiVkoEVHthpWAZUvV/KJ0WJkg+wKbtXJkmMHmSfnpFT4JTLofugBwyQ==", new Guid("a1097867-aa3e-416c-8685-190516b62a12"), "370c4d16-8e11-46ca-9004-e1fb9311e49e", "attendant" },
                    { new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"), new DateTime(2025, 10, 12, 12, 0, 0, 0, DateTimeKind.Utc), "administrator@mechanics.com", "Administrator User", "AQAAAAIAAYagAAAAEPGF9Xsz+ARiCopDgQbQ8gbGubN6bhvNhpKiy8XK2BORE5eV95VywrM9rVE48i2m8w==", new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"), "efcaaf76-0535-45fc-a79c-06ab92c064bb", "administrator" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetItems_BudgetId",
                schema: "Mechanics",
                table: "BudgetItems",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_WorkOrderId",
                schema: "Mechanics",
                table: "Budgets",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Document",
                schema: "Mechanics",
                table: "Customers",
                column: "Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                schema: "Mechanics",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                schema: "Mechanics",
                table: "Customers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                schema: "Mechanics",
                table: "Products",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                schema: "Mechanics",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCatalog_Name",
                schema: "Mechanics",
                table: "ServiceCatalog",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCatalogWorkOrder_WorkOrdersId",
                schema: "Mechanics",
                table: "ServiceCatalogWorkOrder",
                column: "WorkOrdersId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "Mechanics",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FullName",
                schema: "Mechanics",
                table: "Users",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                schema: "Mechanics",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                schema: "Mechanics",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Chassis",
                schema: "Mechanics",
                table: "Vehicles",
                column: "Chassis",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                schema: "Mechanics",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_OwnerId",
                schema: "Mechanics",
                table: "Vehicles",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHistories_WorkOrderId",
                schema: "Mechanics",
                table: "WorkOrderHistories",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderProduct_ProductId",
                schema: "Mechanics",
                table: "WorkOrderProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedToUserId",
                schema: "Mechanics",
                table: "WorkOrders",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId_AccessKey",
                schema: "Mechanics",
                table: "WorkOrders",
                columns: new[] { "CustomerId", "AccessKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_VehicleId",
                schema: "Mechanics",
                table: "WorkOrders",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetItems",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "ServiceCatalogWorkOrder",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "WorkOrderHistories",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "WorkOrderProduct",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "Budgets",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "ServiceCatalog",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "WorkOrders",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "Vehicles",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "Mechanics");
        }
    }
}
