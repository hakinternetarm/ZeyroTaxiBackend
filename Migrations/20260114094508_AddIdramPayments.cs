using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIdramPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PushToken",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarColor",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarMake",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarModel",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarPlate",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CarYear",
                table: "DriverProfiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentLat",
                table: "DriverProfiles",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentLng",
                table: "DriverProfiles",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationAt",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpiry",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseIssuingCountry",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseName",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportCountry",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpiry",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportName",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeAccountId",
                table: "DriverProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IdramPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BillNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PayerAccount = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TransactionDate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdramPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdramPayments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", nullable: true),
                    Last4 = table.Column<string>(type: "TEXT", nullable: false),
                    Brand = table.Column<string>(type: "TEXT", nullable: true),
                    ExpMonth = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpYear = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdramPayments_BillNo",
                table: "IdramPayments",
                column: "BillNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdramPayments_UserId",
                table: "IdramPayments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentCards_UserId_Last4",
                table: "PaymentCards",
                columns: new[] { "UserId", "Last4" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdramPayments");

            migrationBuilder.DropTable(
                name: "PaymentCards");

            migrationBuilder.DropColumn(
                name: "PushToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CarColor",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CarMake",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CarModel",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CarPlate",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CarYear",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentLat",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "CurrentLng",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LastLocationAt",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseExpiry",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseIssuingCountry",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseName",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PassportCountry",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PassportExpiry",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PassportName",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "DriverProfiles");

            migrationBuilder.DropColumn(
                name: "StripeAccountId",
                table: "DriverProfiles");
        }
    }
}
