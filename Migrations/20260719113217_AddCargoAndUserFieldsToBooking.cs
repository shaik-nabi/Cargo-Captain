using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoAndUserFieldsToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "bookingDate",
                table: "ShipmentBookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "cargoDescription",
                table: "ShipmentBookings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "cargoWeight",
                table: "ShipmentBookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "userId",
                table: "ShipmentBookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAECi/X4Jj6Pm28moy0mQqmGF3RNSR/ZH5qhwah1kcxkj9hb1lCGiG3qvtBNss8Jdm5g==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAENEr/Z4Ff0cTQEGCFaY3RI1JJyfCouUND1D68gFYTp96ybfRratfuOhNoeQ3HAtOng==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEBRG8wIlNG2apDtB9MiclVi78eRWQgqodT38ppUxoSrXlg1FEhp5ek6ifWeV9mHQQQ==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEMP7tZE8uGJLtj+z8as0rmxvTTRfKRLDkLtowT4P7jjZOOYEdWBMD8LsblEIwS/83A==");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentBookings_userId",
                table: "ShipmentBookings",
                column: "userId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentBookings_Logins_userId",
                table: "ShipmentBookings",
                column: "userId",
                principalTable: "Logins",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentBookings_Logins_userId",
                table: "ShipmentBookings");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentBookings_userId",
                table: "ShipmentBookings");

            migrationBuilder.DropColumn(
                name: "bookingDate",
                table: "ShipmentBookings");

            migrationBuilder.DropColumn(
                name: "cargoDescription",
                table: "ShipmentBookings");

            migrationBuilder.DropColumn(
                name: "cargoWeight",
                table: "ShipmentBookings");

            migrationBuilder.DropColumn(
                name: "userId",
                table: "ShipmentBookings");

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
    }
}
