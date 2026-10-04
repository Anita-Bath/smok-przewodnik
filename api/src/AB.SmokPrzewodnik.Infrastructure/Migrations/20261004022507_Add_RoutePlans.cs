using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AB.SmokPrzewodnik.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RoutePlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "route_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    request = table.Column<string>(type: "jsonb", nullable: false),
                    alternatives = table.Column<string>(type: "jsonb", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_plans", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_route_plans_account_id",
                table: "route_plans",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_plans_expires_at",
                table: "route_plans",
                column: "expires_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "route_plans");
        }
    }
}
