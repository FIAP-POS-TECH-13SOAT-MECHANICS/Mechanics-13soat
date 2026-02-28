using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RolesForCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "Mechanics",
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("f31bca41-0895-4af5-976f-ac892f833b1b"), "CUSTOMER_ADMIN" },
                    { new Guid("f61b4ae9-cc8f-4fda-a39f-f70bb3c0840f"), "CUSTOMER_USER" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Mechanics",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f31bca41-0895-4af5-976f-ac892f833b1b"));

            migrationBuilder.DeleteData(
                schema: "Mechanics",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("f61b4ae9-cc8f-4fda-a39f-f70bb3c0840f"));
        }
    }
}
