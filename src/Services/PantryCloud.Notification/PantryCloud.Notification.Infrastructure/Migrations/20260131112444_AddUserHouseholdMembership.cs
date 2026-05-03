using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryCloud.Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserHouseholdMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserHouseholdMemberships",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHouseholdMemberships", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserHouseholdMemberships_HouseholdId",
                table: "UserHouseholdMemberships",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHouseholdMemberships_HouseholdId_LeftAt",
                table: "UserHouseholdMemberships",
                columns: new[] { "HouseholdId", "LeftAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserHouseholdMemberships");
        }
    }
}
