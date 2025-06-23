using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: new Guid("826b6481-fe10-4797-8fcf-224ded710b09"));

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "AvailableBalance", "Balance", "CardHolderName", "CardNumber", "Cvv", "ExpiryMonth", "ExpiryYear" },
                values: new object[] { new Guid("513969d5-b7a2-4c06-b5f1-7c81d082fd29"), 0m, 1000.00m, "Test Card Holder", "4242-4242-4242-4242", "123", "12", "2025" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: new Guid("513969d5-b7a2-4c06-b5f1-7c81d082fd29"));

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "AvailableBalance", "Balance", "CardHolderName", "CardNumber", "Cvv", "ExpiryMonth", "ExpiryYear" },
                values: new object[] { new Guid("826b6481-fe10-4797-8fcf-224ded710b09"), 0m, 1000.00m, "Test Card Holder", "4242-4242-4242-4242", "123", "12", "2025" });
        }
    }
}
