using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrderAccessKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CustomerId",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.AddColumn<string>(
                name: "AccessKey",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId_AccessKey",
                schema: "Mechanics",
                table: "WorkOrders",
                columns: new[] { "CustomerId", "AccessKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CustomerId_AccessKey",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "AccessKey",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CustomerId",
                schema: "Mechanics",
                table: "WorkOrders",
                column: "CustomerId");
        }
    }
}
