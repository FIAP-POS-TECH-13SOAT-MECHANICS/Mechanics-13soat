using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOcurredAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccurredAt",
                schema: "Mechanics",
                table: "WorkOrderHistories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAt",
                schema: "Mechanics",
                table: "WorkOrderHistories",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()");
        }
    }
}
