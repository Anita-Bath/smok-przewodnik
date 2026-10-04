using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NavigationSessions_Extend_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_id_account_id",
                table: "navigation_sessions");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_id_hash_account_id",
                table: "navigation_sessions",
                columns: new[] { "id", "hash", "account_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_id_hash_account_id",
                table: "navigation_sessions");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_id_account_id",
                table: "navigation_sessions",
                columns: new[] { "id", "account_id" });
        }
    }
}
