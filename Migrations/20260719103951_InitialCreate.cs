using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CargoCaptain.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logins",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    AssociatedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logins", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentBookings",
                columns: table => new
                {
                    bookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    shipperName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    consigneeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    originPort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    destinationPort = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    bookingStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentBookings", x => x.bookingId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    employeeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    firstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.employeeId);
                    table.ForeignKey(
                        name: "FK_Employees_Logins_userId",
                        column: x => x.userId,
                        principalTable: "Logins",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Containers",
                columns: table => new
                {
                    containerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    containerNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    containerType = table.Column<int>(type: "int", nullable: false),
                    bookingId = table.Column<int>(type: "int", nullable: false),
                    sealNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    containerStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Containers", x => x.containerId);
                    table.ForeignKey(
                        name: "FK_Containers_ShipmentBookings_bookingId",
                        column: x => x.bookingId,
                        principalTable: "ShipmentBookings",
                        principalColumn: "bookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomsDeclarations",
                columns: table => new
                {
                    declarationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingId = table.Column<int>(type: "int", nullable: false),
                    hsCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    declaredValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    calculatedDuty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    clearanceStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomsDeclarations", x => x.declarationId);
                    table.ForeignKey(
                        name: "FK_CustomsDeclarations_ShipmentBookings_bookingId",
                        column: x => x.bookingId,
                        principalTable: "ShipmentBookings",
                        principalColumn: "bookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FreightInvoices",
                columns: table => new
                {
                    invoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingId = table.Column<int>(type: "int", nullable: false),
                    freightCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    surchargeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    demurrageAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    totalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    invoiceStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreightInvoices", x => x.invoiceId);
                    table.ForeignKey(
                        name: "FK_FreightInvoices_ShipmentBookings_bookingId",
                        column: x => x.bookingId,
                        principalTable: "ShipmentBookings",
                        principalColumn: "bookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargoEvents",
                columns: table => new
                {
                    eventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    containerId = table.Column<int>(type: "int", nullable: false),
                    eventType = table.Column<int>(type: "int", nullable: false),
                    eventLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    eventTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoEvents", x => x.eventId);
                    table.ForeignKey(
                        name: "FK_CargoEvents_Containers_containerId",
                        column: x => x.containerId,
                        principalTable: "Containers",
                        principalColumn: "containerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Logins",
                columns: new[] { "UserId", "AssociatedName", "Password", "Role" },
                values: new object[,]
                {
                    { 1, "System Admin", "HASHED_PASSWORD_PLACEHOLDER_FOR_ADMIN", 0 },
                    { 2, "Freight Forwarder Client", "HASHED_PASSWORD_PLACEHOLDER_FOR_FORWARDER", 1 },
                    { 3, "Customs Broker Client", "HASHED_PASSWORD_PLACEHOLDER_FOR_BROKER", 2 },
                    { 4, "Port Operator Client", "HASHED_PASSWORD_PLACEHOLDER_FOR_OPERATOR", 3 }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "employeeId", "email", "firstName", "lastName", "phoneNumber", "userId" },
                values: new object[,]
                {
                    { 1, "admin@cargocaptain.com", "System", "Admin", "+15550100", 1 },
                    { 2, "forwarder@cargocaptain.com", "Freight", "Forwarder", "+15550101", 2 },
                    { 3, "broker@cargocaptain.com", "Customs", "Broker", "+15550102", 3 },
                    { 4, "operator@cargocaptain.com", "Port", "Operator", "+15550103", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoEvents_containerId",
                table: "CargoEvents",
                column: "containerId");

            migrationBuilder.CreateIndex(
                name: "IX_Containers_bookingId",
                table: "Containers",
                column: "bookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Containers_containerNumber",
                table: "Containers",
                column: "containerNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomsDeclarations_bookingId",
                table: "CustomsDeclarations",
                column: "bookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_email",
                table: "Employees",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_userId",
                table: "Employees",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FreightInvoices_bookingId",
                table: "FreightInvoices",
                column: "bookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentBookings_bookingNumber",
                table: "ShipmentBookings",
                column: "bookingNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargoEvents");

            migrationBuilder.DropTable(
                name: "CustomsDeclarations");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "FreightInvoices");

            migrationBuilder.DropTable(
                name: "Containers");

            migrationBuilder.DropTable(
                name: "Logins");

            migrationBuilder.DropTable(
                name: "ShipmentBookings");
        }
    }
}
