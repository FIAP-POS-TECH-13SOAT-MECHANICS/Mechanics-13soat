using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserNameAsCpf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserName",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "CpfNumber",
                schema: "Mechanics",
                table: "Users",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "Mechanics",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"),
                columns: new[] { "CpfNumber", "CustomerId" },
                values: new object[] { "11144477735", null });

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"),
                columns: new[] { "CpfNumber", "CustomerId" },
                values: new object[] { "98765432100", null });

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"),
                columns: new[] { "CpfNumber", "CustomerId" },
                values: new object[] { "12345678909", null });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CpfNumber",
                schema: "Mechanics",
                table: "Users",
                column: "CpfNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CustomerId",
                schema: "Mechanics",
                table: "Users",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Customers_CustomerId",
                schema: "Mechanics",
                table: "Users",
                column: "CustomerId",
                principalSchema: "Mechanics",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Customers_CustomerId",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CpfNumber",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CustomerId",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CpfNumber",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                schema: "Mechanics",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"),
                column: "UserName",
                value: "mechanic");

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"),
                column: "UserName",
                value: "attendant");

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"),
                column: "UserName",
                value: "administrator");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                schema: "Mechanics",
                table: "Users",
                column: "UserName",
                unique: true);
        }
    }
}
