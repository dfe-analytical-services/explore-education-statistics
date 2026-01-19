using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class Ees6798AddPublishedDisplayDateToReleaseVersion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "PublishedDisplayDate",
            table: "ReleaseVersions",
            type: "datetimeoffset",
            nullable: true
        );

        // Set PublishedDisplayDate as a copy of Published for all existing ReleaseVersions
        migrationBuilder.Sql("UPDATE ReleaseVersions SET PublishedDisplayDate = Published WHERE Published IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PublishedDisplayDate", table: "ReleaseVersions");
    }
}
