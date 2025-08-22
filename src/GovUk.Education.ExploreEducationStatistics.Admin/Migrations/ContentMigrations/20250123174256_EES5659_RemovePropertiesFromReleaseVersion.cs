#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5659_RemovePropertiesFromReleaseVersion : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReleaseName",
            table: "ReleaseVersions");

        migrationBuilder.DropColumn(
            name: "Slug",
            table: "ReleaseVersions");

        migrationBuilder.DropColumn(
            name: "TimePeriodCoverage",
            table: "ReleaseVersions");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ReleaseName",
            table: "ReleaseVersions",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Slug",
            table: "ReleaseVersions",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "TimePeriodCoverage",
            table: "ReleaseVersions",
            type: "nvarchar(6)",
            maxLength: 6,
            nullable: false,
            defaultValue: "");
    }
}
