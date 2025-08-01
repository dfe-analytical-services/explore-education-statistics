using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5158_UpdateNationalStatisticsReleaseType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE ReleaseVersions SET Type = 'AccreditedOfficialStatistics' WHERE Type = 'NationalStatistics'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "UPDATE ReleaseVersions SET Type = 'NationalStatistics' WHERE Type = 'AccreditedOfficialStatistics'");

    }
}
