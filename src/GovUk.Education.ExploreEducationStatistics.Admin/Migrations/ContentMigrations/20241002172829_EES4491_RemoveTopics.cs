#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES4491_RemoveTopics : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Publications_Topics_TopicId",
            table: "Publications"
        );

        migrationBuilder.DropTable(name: "Topics");

        migrationBuilder.DropIndex(name: "IX_Publications_TopicId", table: "Publications");

        migrationBuilder.DropColumn(name: "TopicId", table: "Publications");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "TopicId",
            table: "Publications",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000")
        );

        migrationBuilder.CreateTable(
            name: "Topics",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ThemeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Topics", x => x.Id);
                table.ForeignKey(
                    name: "FK_Topics_Themes_ThemeId",
                    column: x => x.ThemeId,
                    principalTable: "Themes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_Publications_TopicId",
            table: "Publications",
            column: "TopicId"
        );

        migrationBuilder.CreateIndex(name: "IX_Topics_ThemeId", table: "Topics", column: "ThemeId");

        migrationBuilder.AddForeignKey(
            name: "FK_Publications_Topics_TopicId",
            table: "Publications",
            column: "TopicId",
            principalTable: "Topics",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );
    }
}
