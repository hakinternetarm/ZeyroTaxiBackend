using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxi_API.Migrations
{
    public partial class AddScheduledPlanTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduledPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    EntriesJson = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledPlanExecutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    OccurrenceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledPlanExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledPlanExecutions_ScheduledPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "ScheduledPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlans_UserId",
                table: "ScheduledPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanExecutions_PlanId",
                table: "ScheduledPlanExecutions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledPlanExecutions_PlanId_EntryIndex_OccurrenceDate",
                table: "ScheduledPlanExecutions",
                columns: new[] { "PlanId", "EntryIndex", "OccurrenceDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledPlanExecutions");

            migrationBuilder.DropTable(
                name: "ScheduledPlans");
        }
    }
}
