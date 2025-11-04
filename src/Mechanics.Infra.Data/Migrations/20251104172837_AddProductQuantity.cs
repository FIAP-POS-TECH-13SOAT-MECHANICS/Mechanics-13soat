using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderProducts",
                schema: "Mechanics");

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

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderProduct_ProductId",
                schema: "Mechanics",
                table: "WorkOrderProduct",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderProduct",
                schema: "Mechanics");

            migrationBuilder.CreateTable(
                name: "WorkOrderProducts",
                schema: "Mechanics",
                columns: table => new
                {
                    ProductsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkOrdersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderProducts", x => new { x.ProductsId, x.WorkOrdersId });
                    table.ForeignKey(
                        name: "FK_WorkOrderProducts_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalSchema: "Mechanics",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkOrderProducts_WorkOrders_WorkOrdersId",
                        column: x => x.WorkOrdersId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderProducts_WorkOrdersId",
                schema: "Mechanics",
                table: "WorkOrderProducts",
                column: "WorkOrdersId");
        }
    }
}
