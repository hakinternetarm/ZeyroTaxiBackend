using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaxiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledPlanTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with IF NOT EXISTS so this migration is safe to apply on databases that
            // may already have some of these tables/indexes (common in development).
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ScheduledPlans (
                    Id TEXT PRIMARY KEY,
                    UserId TEXT NOT NULL,
                    Name TEXT,
                    EntriesJson TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ScheduledPlanExecutions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlanId TEXT NOT NULL,
                    EntryIndex INTEGER NOT NULL,
                    OccurrenceDate TEXT NOT NULL,
                    ExecutedAt TEXT NOT NULL
                );
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_ScheduledPlans_UserId ON ScheduledPlans(UserId);
            ");

            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX IF NOT EXISTS IX_ScheduledPlanExecutions_PlanId_EntryIndex_OccurrenceDate 
                ON ScheduledPlanExecutions(PlanId, EntryIndex, OccurrenceDate);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_ScheduledPlanExecutions_PlanId_EntryIndex_OccurrenceDate;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_ScheduledPlans_UserId;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS ScheduledPlanExecutions;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS ScheduledPlans;");
        }
    }
}
