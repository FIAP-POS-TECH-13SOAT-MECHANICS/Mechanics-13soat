using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBudgetAndWorkOrderCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "Mechanics",
                table: "Budgets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "Mechanics",
                table: "Budgets",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()");
        }
    }
}
