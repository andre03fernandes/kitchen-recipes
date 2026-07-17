using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenRecipes.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AppUsers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "AppUsers");
        }
    }
}
