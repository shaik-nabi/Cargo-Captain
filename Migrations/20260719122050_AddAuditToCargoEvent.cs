using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditToCargoEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "createdDate",
                table: "CargoEvents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "recordedBy",
                table: "CargoEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdDate",
                table: "CargoEvents");

            migrationBuilder.DropColumn(
                name: "recordedBy",
                table: "CargoEvents");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEEMZtwMf1ehhIBj/XK30KFlPGv8RsXpSw+U4r8P5cSkhAek/IflyheMbi2NhXNOwlw==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 2,
                column: "Password",
                value: "AQAAAAIAAYagAAAAENEhNg5d3ZQVHTTwoyXfL96RUEcSRxPdZYZIFTCeWJ+xflcFephZdjqiKJKEWuG6mQ==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 3,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEHjiu6FISCw5PlAm0v3tpAIiXzf+9Wo+QI/rv81dDQXw668bRjaDWe6U7m/kjFKieQ==");

            migrationBuilder.UpdateData(
                table: "Logins",
                keyColumn: "UserId",
                keyValue: 4,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEGVs6tBSkSCsKQWbpc5o+BrwNdq76ub9q1Xu5ovNU+SonBM1560jVgP9Khbh+2WR0g==");
        }
    }
}
