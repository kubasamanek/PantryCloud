using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryCloud.Pantry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable pgcrypto extension for gen_random_bytes function
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");
            
            migrationBuilder.CreateTable(
                name: "PantryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false, defaultValueSql: "gen_random_bytes(8)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PantryItems", x => x.Id);
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
                name: "IX_PantryItems_HouseholdId",
                table: "PantryItems",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_HouseholdId_Category",
                table: "PantryItems",
                columns: new[] { "HouseholdId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_HouseholdId_Name",
                table: "PantryItems",
                columns: new[] { "HouseholdId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_UserHouseholdMemberships_HouseholdId",
                table: "UserHouseholdMemberships",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHouseholdMemberships_HouseholdId_LeftAt",
                table: "UserHouseholdMemberships",
                columns: new[] { "HouseholdId", "LeftAt" });
            
            // Create trigger to ensure RowVersion is always set, even if EF Core tries to insert NULL
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION set_pantry_item_rowversion()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF NEW.""RowVersion"" IS NULL THEN
                        NEW.""RowVersion"" := gen_random_bytes(8);
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                
                CREATE TRIGGER trg_set_pantry_item_rowversion
                    BEFORE INSERT OR UPDATE ON ""PantryItems""
                    FOR EACH ROW
                    EXECUTE FUNCTION set_pantry_item_rowversion();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_set_pantry_item_rowversion ON ""PantryItems"";
                DROP FUNCTION IF EXISTS set_pantry_item_rowversion();
            ");
            
            migrationBuilder.DropTable(
                name: "PantryItems");

            migrationBuilder.DropTable(
                name: "UserHouseholdMemberships");
        }
    }
}
