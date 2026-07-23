using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEKtDlnA/ICl/IgWKiqhGBAFQ5WznU4VB2HxQxCUhBrj+L64b7E5k7GlniLa7ma0lNA==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEBeXmPJins5G8szmnWEbEh9i0w2rc1/jnvvOsPaKhmWAxZCB2cCGrlxOGnlgW3hjSA==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEAgDIr9AF/rnP9vOo2dhK1eGqYSg66PKxAGa9tGsgzucsOx7ykSvq/iGZB/xgLkmAA==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEDKQb8KmwTWzCxsXMx+gDt/3cX7r4un3KFP6ieSCBzd6S9+4arLM4lRHQ72D79Ficg==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "HASHED_PASSWORD_PLACEHOLDER_FOR_ADMIN");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "HASHED_PASSWORD_PLACEHOLDER_FOR_FORWARDER");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "HASHED_PASSWORD_PLACEHOLDER_FOR_BROKER");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "HASHED_PASSWORD_PLACEHOLDER_FOR_OPERATOR");
        }
    }
}
