using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDetailsToBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "invoiceNumber",
                table: "FreightInvoices",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "paymentDate",
                table: "FreightInvoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAELq+sWdoUcRBNA7YBmmrkzMkx2B18MGJB0cxBiBMMQ6HTuQvmzZGHpEYI6ClYIUZ5w==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEI0pW4rGUjdDIFe5kQQO024QCiZIUOR+SMjdMuzUBpvE7kjWp5dVZbQOwuYu96+nJQ==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEAwlxkM9zfU5phBb3qNsacDj/KynaIBchVjT5lQBiB02dJo8xH75TLZZl2rl23QwJw==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEPnI5sX1n+27b3XRcVpU8ZMYy01RM+CEgZ7dZXk8qFGntyQEN1wJPFkTQfT6cu/Oog==");

            migrationBuilder.CreateIndex(
                name: "IX_FreightInvoices_invoiceNumber",
                table: "FreightInvoices",
                column: "invoiceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FreightInvoices_invoiceNumber",
                table: "FreightInvoices");

            migrationBuilder.DropColumn(
                name: "invoiceNumber",
                table: "FreightInvoices");

            migrationBuilder.DropColumn(
                name: "paymentDate",
                table: "FreightInvoices");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEBEo4umrI8c6lBnaLSszRAtebkxRVzRUGO8gBG1ZjBW5JqTler+BckODyw2tUcvFgA==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAECcKVf5aHAifBWbKeOrLOxWBRM+VrJ6Rh7zPGSfFAWlGS8YxtbwEFxhaaM0ZXKodWw==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAELSTger7+1JLDivOlmR6teH205gQTZKpxwzUDoP6dgm1Ld4sMpTg1cE7PNR6xwJk1Q==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAELIEDwJqs3G1ar43Gsk/MRt2438jh3zdQQ7+RbtJq14cSKs97XiINK51DRBRUOtFTQ==");
        }
    }
}
