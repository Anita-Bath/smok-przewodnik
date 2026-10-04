using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NavigationSessions_Extend_Index_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_account_id",
                table: "navigation_sessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_account_id",
                table: "navigation_sessions",
                column: "account_id",
                unique: true,
                filter: "account_id IS NOT NULL");
        }
    }
}
