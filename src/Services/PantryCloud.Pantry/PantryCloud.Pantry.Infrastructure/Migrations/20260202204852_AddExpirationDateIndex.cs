using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryCloud.Pantry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpirationDateIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PantryItems_HouseholdId_ExpirationDate",
                table: "PantryItems",
                columns: new[] { "HouseholdId", "ExpirationDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PantryItems_HouseholdId_ExpirationDate",
                table: "PantryItems");
        }
    }
}
