using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class AddDemurragePaymentFieldsToFreightInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "demurragePaidByUserId",
                table: "FreightInvoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "demurragePaymentDate",
                table: "FreightInvoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "demurrageStatus",
                table: "FreightInvoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEKR59ti0V+sYObbT3sy/zaSFUIlCleMgDJlSeRR7cGN9n0u/9iXLg36GZCwfTcSFmw==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAENGkLuAc1Eox4qPApnzDCbQxPgsxb+abT16188JB/NV5GaY76bFfVz7q7hNkvb/5cg==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEHNR/hLEESygxohAvKeZy30F4erknupDznvp6JUdNfObHgdnn6YQ6QvNiX9URrXhyQ==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEFq52DzrPhOJg0QvZ16VzN/bKfLAeY1wtEmA/dmcrP2ZUgpzT/n2brNgLE1aba8ubw==");

            migrationBuilder.CreateIndex(
                name: "IX_FreightInvoices_demurragePaidByUserId",
                table: "FreightInvoices",
                column: "demurragePaidByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FreightInvoices_Logins_demurragePaidByUserId",
                table: "FreightInvoices",
                column: "demurragePaidByUserId",
                principalTable: "Logins",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FreightInvoices_Logins_demurragePaidByUserId",
                table: "FreightInvoices");

            migrationBuilder.DropIndex(
                name: "IX_FreightInvoices_demurragePaidByUserId",
                table: "FreightInvoices");

            migrationBuilder.DropColumn(
                name: "demurragePaidByUserId",
                table: "FreightInvoices");

            migrationBuilder.DropColumn(
                name: "demurragePaymentDate",
                table: "FreightInvoices");

            migrationBuilder.DropColumn(
                name: "demurrageStatus",
                table: "FreightInvoices");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAECuo1sV5thnLfBK08+0IlNKORsKfwZ6IW4YNWetc5N9eWteXUQb+vdg9d8YKiRsPOg==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEHpO57r3N4g1iJjvZX3VHqZ4Ln9iEROTTv3eqPtlBKXANb/pt27zMdOHb+aYZ213TA==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEMb7neXBcOYDfMbSpsDOM1xsZ/yzKGzkniq07RHUYDO+OzQ09YRl9Urn2ZDw4SewOQ==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEDJ+DWKxtqxX25Z0xB/NAGyHm/dATgmn0fxlk7rU3cgoDptnJvWNDlh8qSOc04ScRw==");
        }
    }
}
