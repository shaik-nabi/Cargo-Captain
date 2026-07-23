using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class AddDeclarationTypeToCustoms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "declarationType",
                table: "CustomsDeclarations",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "declarationType",
                table: "CustomsDeclarations");

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
        }
    }
}
