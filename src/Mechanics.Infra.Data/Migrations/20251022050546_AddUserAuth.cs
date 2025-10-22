using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "Mechanics",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "Mechanics",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                schema: "Mechanics",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                schema: "Mechanics",
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Email", "FullName", "PasswordHash", "RoleId", "SecurityStamp", "UserName" },
                values: new object[,]
                {
                    { new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"), new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "mechanic@mechanics.com", "Mechanic User", "AQAAAAIAAYagAAAAEKSeHdHtCfN38pakeil4oyEL0d07GBEySe6csY8jmXIKT3oEZVcZR7Jngd9qxFgmkQ==", new Guid("f6027484-89a4-49f6-a9cb-4d1733c2bab7"), "0a3bc211-1220-4d20-80e9-bd850d0dc200", "mechanic" },
                    { new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"), new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "attendant@mechanics.com", "Attendant User", "AQAAAAIAAYagAAAAEEo/VptbCYVPiVkoEVHthpWAZUvV/KJ0WJkg+wKbtXJkmMHmSfnpFT4JTLofugBwyQ==", new Guid("a1097867-aa3e-416c-8685-190516b62a12"), "370c4d16-8e11-46ca-9004-e1fb9311e49e", "attendant" },
                    { new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"), new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@mechanics.com", "Administrator User", "AQAAAAIAAYagAAAAEPGF9Xsz+ARiCopDgQbQ8gbGubN6bhvNhpKiy8XK2BORE5eV95VywrM9rVE48i2m8w==", new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"), "efcaaf76-0535-45fc-a79c-06ab92c064bb", "admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                schema: "Mechanics",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DeleteData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"));

            migrationBuilder.DeleteData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"));

            migrationBuilder.DeleteData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"));

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "Mechanics",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                schema: "Mechanics",
                table: "Users");
        }
    }
}
