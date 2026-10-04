using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_NavigationProgressAndEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "navigation_events",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<long>(type: "bigint", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    urgency = table.Column<string>(type: "text", nullable: false),
                    maneuver = table.Column<string>(type: "jsonb", nullable: true),
                    hazard_code = table.Column<string>(type: "text", nullable: true),
                    landmark_code = table.Column<string>(type: "text", nullable: true),
                    remaining_distance_metres = table.Column<decimal>(type: "numeric", nullable: true),
                    localized_parameters = table.Column<string>(type: "jsonb", nullable: false),
                    supported_feedback_patterns = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_navigation_events", x => new { x.session_id, x.sequence });
                    table.ForeignKey(
                        name: "FK_navigation_events_navigation_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "navigation_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "navigation_events");
        }
    }
}
