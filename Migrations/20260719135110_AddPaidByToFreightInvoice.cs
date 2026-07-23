using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidByToFreightInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "paidByUserId",
                table: "FreightInvoices",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_FreightInvoices_paidByUserId",
                table: "FreightInvoices",
                column: "paidByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FreightInvoices_Logins_paidByUserId",
                table: "FreightInvoices",
                column: "paidByUserId",
                principalTable: "Logins",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FreightInvoices_Logins_paidByUserId",
                table: "FreightInvoices");

            migrationBuilder.DropIndex(
                name: "IX_FreightInvoices_paidByUserId",
                table: "FreightInvoices");

            migrationBuilder.DropColumn(
                name: "paidByUserId",
                table: "FreightInvoices");

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
        }
    }
}
