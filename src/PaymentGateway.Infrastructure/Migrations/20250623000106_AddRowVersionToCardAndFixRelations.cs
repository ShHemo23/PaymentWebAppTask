using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionToCardAndFixRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: new Guid("f9a4a7a0-02a8-4e3a-8671-5f2a1d7f6b8a"));

            migrationBuilder.AddColumn<string>(
                name: "PublicTransactionId",
                table: "Transactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RefundCode",
                table: "Transactions",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RefundCodeExpiryUtc",
                table: "Transactions",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "Cards",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(19)",
                oldMaxLength: 19);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableBalance",
                table: "Cards",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Cards",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "AvailableBalance", "Balance", "CardHolderName", "CardNumber", "Cvv", "ExpiryMonth", "ExpiryYear" },
                values: new object[] { new Guid("826b6481-fe10-4797-8fcf-224ded710b09"), 0m, 1000.00m, "Test Card Holder", "4242-4242-4242-4242", "123", "12", "2025" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cards",
                keyColumn: "Id",
                keyValue: new Guid("826b6481-fe10-4797-8fcf-224ded710b09"));

            migrationBuilder.DropColumn(
                name: "PublicTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RefundCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RefundCodeExpiryUtc",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AvailableBalance",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Cards");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "Cards",
                type: "nvarchar(19)",
                maxLength: 19,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.InsertData(
                table: "Cards",
                columns: new[] { "Id", "Balance", "CardHolderName", "CardNumber", "Cvv", "ExpiryMonth", "ExpiryYear" },
                values: new object[] { new Guid("f9a4a7a0-02a8-4e3a-8671-5f2a1d7f6b8a"), 1000.00m, "John Smith", "4242-4242-4242-4242", "123", "12", "2030" });
        }
    }
}
