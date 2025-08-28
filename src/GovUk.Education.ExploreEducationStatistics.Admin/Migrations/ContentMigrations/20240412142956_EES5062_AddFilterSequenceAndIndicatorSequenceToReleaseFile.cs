#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES5062_AddFilterSequenceAndIndicatorSequenceToReleaseFile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "FilterSequence",
            table: "ReleaseFiles",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "IndicatorSequence",
            table: "ReleaseFiles",
            type: "nvarchar(max)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FilterSequence",
            table: "ReleaseFiles");

        migrationBuilder.DropColumn(
            name: "IndicatorSequence",
            table: "ReleaseFiles");
    }
}
