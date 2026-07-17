using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenRecipes.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsletterSqlObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterCampaignAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientsCount = table.Column<int>(type: "int", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterCampaignAudits", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[AppUserRoleAudits]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[AppUserRoleAudits]
                    (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [AppUserId] INT NOT NULL,
                        [OldRole] NVARCHAR(32) NOT NULL,
                        [NewRole] NVARCHAR(32) NOT NULL,
                        [ChangedAtUtc] DATETIME2 NOT NULL
                    );
                END;
                """);

            migrationBuilder.Sql(
                """
                CREATE OR ALTER TRIGGER [dbo].[trg_AppUsers_RoleChange_Audit]
                ON [dbo].[AppUsers]
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF UPDATE([Role])
                    BEGIN
                        INSERT INTO [dbo].[AppUserRoleAudits] ([AppUserId], [OldRole], [NewRole], [ChangedAtUtc])
                        SELECT
                            i.[Id],
                            d.[Role],
                            i.[Role],
                            SYSUTCDATETIME()
                        FROM inserted i
                        INNER JOIN deleted d ON d.[Id] = i.[Id]
                        WHERE ISNULL(d.[Role], '') <> ISNULL(i.[Role], '');
                    END
                END;
                """);

            migrationBuilder.Sql(
                """
                CREATE OR ALTER PROCEDURE [dbo].[usp_GetNewsletterCampaignStats]
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT
                        TotalSubscribers = COUNT(1),
                        ConfirmedSubscribers = SUM(CASE WHEN [IsConfirmed] = 1 THEN 1 ELSE 0 END),
                        TotalCampaignsSent = ISNULL((SELECT COUNT(1) FROM [dbo].[NewsletterCampaignAudits]), 0),
                        LastCampaignSentAtUtc = (SELECT MAX([SentAtUtc]) FROM [dbo].[NewsletterCampaignAudits])
                    FROM [dbo].[NewsletterSubscribers];
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[usp_GetNewsletterCampaignStats]', N'P') IS NOT NULL
                BEGIN
                    DROP PROCEDURE [dbo].[usp_GetNewsletterCampaignStats];
                END;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[trg_AppUsers_RoleChange_Audit]', N'TR') IS NOT NULL
                BEGIN
                    DROP TRIGGER [dbo].[trg_AppUsers_RoleChange_Audit];
                END;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[AppUserRoleAudits]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [dbo].[AppUserRoleAudits];
                END;
                """);

            migrationBuilder.DropTable(
                name: "NewsletterCampaignAudits");
        }
    }
}
