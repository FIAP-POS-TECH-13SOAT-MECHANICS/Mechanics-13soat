using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Mechanics.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderFieldsAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalRequestedAt",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LastStatusChangeBy",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observations",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportedProblem",
                schema: "Mechanics",
                table: "WorkOrders",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkOrderHistories",
                schema: "Mechanics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    WorkOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrderHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrderHistories_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalSchema: "Mechanics",
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "Mechanics",
                table: "Users",
                columns: new[] { "Id", "CreationDate", "Email", "FullName", "PasswordHash", "RoleId", "SecurityStamp", "UserName" },
                values: new object[,]
                {
                    { new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"), new DateTime(2025, 10, 12, 12, 0, 0, 0, DateTimeKind.Utc), "mechanic@mechanics.com", "Mechanic User", "AQAAAAIAAYagAAAAEKSeHdHtCfN38pakeil4oyEL0d07GBEySe6csY8jmXIKT3oEZVcZR7Jngd9qxFgmkQ==", new Guid("f6027484-89a4-49f6-a9cb-4d1733c2bab7"), "0a3bc211-1220-4d20-80e9-bd850d0dc200", "mechanic" },
                    { new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"), new DateTime(2025, 10, 12, 12, 0, 0, 0, DateTimeKind.Utc), "attendant@mechanics.com", "Attendant User", "AQAAAAIAAYagAAAAEEo/VptbCYVPiVkoEVHthpWAZUvV/KJ0WJkg+wKbtXJkmMHmSfnpFT4JTLofugBwyQ==", new Guid("a1097867-aa3e-416c-8685-190516b62a12"), "370c4d16-8e11-46ca-9004-e1fb9311e49e", "attendant" },
                    { new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"), new DateTime(2025, 10, 12, 12, 0, 0, 0, DateTimeKind.Utc), "administrator@mechanics.com", "Administrator User", "AQAAAAIAAYagAAAAEPGF9Xsz+ARiCopDgQbQ8gbGubN6bhvNhpKiy8XK2BORE5eV95VywrM9rVE48i2m8w==", new Guid("2afde195-550b-498e-a63d-7a6d556b25ba"), "efcaaf76-0535-45fc-a79c-06ab92c064bb", "administrator" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderHistories_WorkOrderId",
                schema: "Mechanics",
                table: "WorkOrderHistories",
                column: "WorkOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkOrderHistories",
                schema: "Mechanics");

            migrationBuilder.DropColumn(
                name: "ApprovalRequestedAt",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "LastStatusChangeBy",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Observations",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ReportedProblem",
                schema: "Mechanics",
                table: "WorkOrders");

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("4c3b8777-6c4a-4bf3-8ad5-aad48981f7f2"),
                column: "CreationDate",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("c2a83e5a-27c7-440a-97e3-86234eebb3c7"),
                column: "CreationDate",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "Mechanics",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("db27b85d-b0f3-4300-bb45-7841f0d11617"),
                column: "CreationDate",
                value: new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
