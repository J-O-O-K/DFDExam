using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AnalyticsService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateMaterializedView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "task_metrics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_tasks = table.Column<int>(type: "integer", nullable: false),
                    completed_tasks = table.Column<int>(type: "integer", nullable: false),
                    overdue_tasks = table.Column<int>(type: "integer", nullable: false),
                    average_completion_time_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    completion_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_metrics", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_metrics_date",
                table: "task_metrics",
                column: "date");

            // Create the materialized view
            var sql = @"
DROP MATERIALIZED VIEW IF EXISTS mv_daily_task_metrics CASCADE;

CREATE MATERIALIZED VIEW mv_daily_task_metrics AS
SELECT 
    ROW_NUMBER() OVER (ORDER BY date DESC) as id,
    date,
    total_tasks,
    completed_tasks,
    overdue_tasks,
    average_completion_time_hours,
    CASE 
        WHEN total_tasks > 0 
        THEN ROUND((completed_tasks::decimal / total_tasks::decimal) * 100, 2)
        ELSE 0.0 
    END as completion_rate
FROM task_metrics
ORDER BY date DESC;

CREATE UNIQUE INDEX idx_mv_daily_task_metrics_id ON mv_daily_task_metrics(id);
CREATE INDEX idx_mv_daily_task_metrics_date ON mv_daily_task_metrics(date DESC);

GRANT SELECT ON mv_daily_task_metrics TO postgres;";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_daily_task_metrics CASCADE;");
            
            migrationBuilder.DropTable(
                name: "task_metrics");
        }
    }
}
