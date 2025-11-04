using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedToWorkOrderFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_AssignedToUserId",
                schema: "Mechanics",
                table: "WorkOrders",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_AssignedToUserId",
                schema: "Mechanics",
                table: "WorkOrders",
                column: "AssignedToUserId",
                principalSchema: "Mechanics",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_AssignedToUserId",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_AssignedToUserId",
                schema: "Mechanics",
                table: "WorkOrders");
        }
    }
}
