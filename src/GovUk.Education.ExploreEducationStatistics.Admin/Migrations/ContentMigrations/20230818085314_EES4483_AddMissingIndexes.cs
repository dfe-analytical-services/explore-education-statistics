using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

public partial class EES4483_AddMissingIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "Files",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "ContentSections",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "ContentBlock",
            type: "nvarchar(25)",
            maxLength: 25,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_Files_Type",
            table: "Files",
            column: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_ContentSections_Type",
            table: "ContentSections",
            column: "Type");

        migrationBuilder.CreateIndex(
            name: "IX_ContentBlock_Type",
            table: "ContentBlock",
            column: "Type");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Files_Type",
            table: "Files");

        migrationBuilder.DropIndex(
            name: "IX_ContentSections_Type",
            table: "ContentSections");

        migrationBuilder.DropIndex(
            name: "IX_ContentBlock_Type",
            table: "ContentBlock");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "Files",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25);

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "ContentSections",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25);

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "ContentBlock",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(25)",
            oldMaxLength: 25);
    }
}
