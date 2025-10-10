using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class CustomerEmailAndVehiclesColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Plate",
                schema: "Mechanics",
                table: "Vehicles",
                newName: "LicensePlate");

            migrationBuilder.RenameColumn(
                name: "Brand",
                schema: "Mechanics",
                table: "Vehicles",
                newName: "Manufacturer");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_Plate",
                schema: "Mechanics",
                table: "Vehicles",
                newName: "IX_Vehicles_LicensePlate");

            migrationBuilder.RenameColumn(
                name: "FullName",
                schema: "Mechanics",
                table: "Customers",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_FullName",
                schema: "Mechanics",
                table: "Customers",
                newName: "IX_Customers_Name");

            migrationBuilder.AddColumn<string>(
                name: "Chassis",
                schema: "Mechanics",
                table: "Vehicles",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "Mechanics",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Chassis",
                schema: "Mechanics",
                table: "Vehicles",
                column: "Chassis",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                schema: "Mechanics",
                table: "Customers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vehicles_Chassis",
                schema: "Mechanics",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                schema: "Mechanics",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Chassis",
                schema: "Mechanics",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "Mechanics",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "LicensePlate",
                schema: "Mechanics",
                table: "Vehicles",
                newName: "Plate");

            migrationBuilder.RenameColumn(
                name: "Manufacturer",
                schema: "Mechanics",
                table: "Vehicles",
                newName: "Brand");

            migrationBuilder.RenameIndex(
                name: "IX_Vehicles_LicensePlate",
                schema: "Mechanics",
                table: "Vehicles",
                newName: "IX_Vehicles_Plate");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "Mechanics",
                table: "Customers",
                newName: "FullName");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_Name",
                schema: "Mechanics",
                table: "Customers",
                newName: "IX_Customers_FullName");
        }
    }
}
