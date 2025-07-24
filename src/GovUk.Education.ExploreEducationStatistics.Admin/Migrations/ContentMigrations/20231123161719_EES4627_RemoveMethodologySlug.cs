#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4627_RemoveMethodologySlug : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Slug",
            table: "Methodologies");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Slug",
            table: "Methodologies",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
