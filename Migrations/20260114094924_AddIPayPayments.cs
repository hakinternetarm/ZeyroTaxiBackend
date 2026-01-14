using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIPayPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IPayPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrderNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IPayOrderId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Pan = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CardholderName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ApprovalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ActionCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ActionCodeDescription = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IPayPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IPayPayments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IPayPayments_IPayOrderId",
                table: "IPayPayments",
                column: "IPayOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_IPayPayments_OrderNumber",
                table: "IPayPayments",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IPayPayments_UserId",
                table: "IPayPayments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IPayPayments");
        }
    }
}
