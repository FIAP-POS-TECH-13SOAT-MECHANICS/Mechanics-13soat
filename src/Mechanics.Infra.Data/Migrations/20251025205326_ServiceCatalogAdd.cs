using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class ServiceCatalogAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceCatalog",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    AverageTime = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCatalog", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCatalogWorkOrder_WorkOrdersId",
                schema: "Mechanics",
                table: "ServiceCatalogWorkOrder",
                column: "WorkOrdersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceCatalogWorkOrder",
                schema: "Mechanics");

            migrationBuilder.DropTable(
                name: "ServiceCatalog",
                schema: "Mechanics");
        }
    }
}
