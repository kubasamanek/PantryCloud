using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryCloud.Audit.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HouseholdAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Payload = table.Column<string>(type: "text", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdAuditEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserHouseholdMemberships",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHouseholdMemberships", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdAuditEntries_EventId",
                table: "HouseholdAuditEntries",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdAuditEntries_HouseholdId_OccurredAt",
                table: "HouseholdAuditEntries",
                columns: new[] { "HouseholdId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdAuditEntries_OccurredAt",
                table: "HouseholdAuditEntries",
                column: "OccurredAt");

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
                name: "HouseholdAuditEntries");

            migrationBuilder.DropTable(
                name: "UserHouseholdMemberships");
        }
    }
}
