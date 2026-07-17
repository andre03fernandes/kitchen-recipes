using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenRecipes.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistantChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssistantChatThreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    LastMessagePreview = table.Column<string>(type: "nvarchar(240)", maxLength: 240, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantChatThreads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssistantChatThreads_AppUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssistantChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssistantChatThreadId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    PantryHighlightsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    SuggestedRecipesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 16000, nullable: true),
                    FollowUpPromptsJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistantChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssistantChatMessages_AssistantChatThreads_AssistantChatThreadId",
                        column: x => x.AssistantChatThreadId,
                        principalTable: "AssistantChatThreads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssistantChatMessages_AssistantChatThreadId_CreatedAtUtc",
                table: "AssistantChatMessages",
                columns: new[] { "AssistantChatThreadId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AssistantChatThreads_AppUserId_UpdatedAtUtc",
                table: "AssistantChatThreads",
                columns: new[] { "AppUserId", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AssistantChatThreads_CreatedAtUtc",
                table: "AssistantChatThreads",
                column: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssistantChatMessages");

            migrationBuilder.DropTable(
                name: "AssistantChatThreads");
        }
    }
}
