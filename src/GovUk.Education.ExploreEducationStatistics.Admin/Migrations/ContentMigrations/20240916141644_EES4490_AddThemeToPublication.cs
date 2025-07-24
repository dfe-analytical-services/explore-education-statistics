#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES4490_AddThemeToPublication : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ThemeId",
            table: "Publications",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateIndex(
            name: "IX_Publications_ThemeId",
            table: "Publications",
            column: "ThemeId");

        migrationBuilder.Sql(@"
                UPDATE Publications
                SET ThemeId = Topics.ThemeId
                FROM Publications
                JOIN Topics
                ON Topics.Id = Publications.TopicId");

        migrationBuilder.AddForeignKey(
            name: "FK_Publications_Themes_ThemeId",
            table: "Publications",
            column: "ThemeId",
            principalTable: "Themes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Publications_Themes_ThemeId",
            table: "Publications");

        migrationBuilder.DropIndex(
            name: "IX_Publications_ThemeId",
            table: "Publications");

        migrationBuilder.DropColumn(
            name: "ThemeId",
            table: "Publications");
    }
}
