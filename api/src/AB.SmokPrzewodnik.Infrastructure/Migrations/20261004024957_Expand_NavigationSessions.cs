using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Expand_NavigationSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Navigation sessions are short-lived and the old shape cannot be upgraded safely.
            migrationBuilder.Sql("DELETE FROM navigation_sessions;");

            migrationBuilder.AddColumn<string>(
                name: "active_route",
                table: "navigation_sessions",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'null'::jsonb");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "navigation_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "effective_route_request",
                table: "navigation_sessions",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'null'::jsonb");

            migrationBuilder.AddColumn<string>(
                name: "latest_progress",
                table: "navigation_sessions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "next_event_sequence",
                table: "navigation_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_at",
                table: "navigation_sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "version",
                table: "navigation_sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_account_id",
                table: "navigation_sessions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_expires_at",
                table: "navigation_sessions",
                column: "expires_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_account_id",
                table: "navigation_sessions");

            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_expires_at",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "active_route",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "effective_route_request",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "latest_progress",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "next_event_sequence",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "navigation_sessions");

            migrationBuilder.DropColumn(
                name: "version",
                table: "navigation_sessions");
        }
    }
}
