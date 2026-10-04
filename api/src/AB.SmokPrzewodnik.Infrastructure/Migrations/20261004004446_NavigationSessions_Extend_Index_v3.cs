using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NavigationSessions_Extend_Index_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_id_hash_account_id",
                table: "navigation_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "hash",
                table: "navigation_sessions",
                type: "character varying(43)",
                maxLength: 43,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_account_id",
                table: "navigation_sessions",
                column: "account_id",
                unique: true,
                filter: "account_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_hash",
                table: "navigation_sessions",
                column: "hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_account_id",
                table: "navigation_sessions");

            migrationBuilder.DropIndex(
                name: "IX_navigation_sessions_hash",
                table: "navigation_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "hash",
                table: "navigation_sessions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(43)",
                oldMaxLength: 43);

            migrationBuilder.CreateIndex(
                name: "IX_navigation_sessions_id_hash_account_id",
                table: "navigation_sessions",
                columns: new[] { "id", "hash", "account_id" },
                unique: true);
        }
    }
}
