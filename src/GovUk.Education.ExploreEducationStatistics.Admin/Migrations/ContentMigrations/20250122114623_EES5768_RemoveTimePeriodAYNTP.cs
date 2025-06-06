using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES5768_RemoveTimePeriodAYNTP : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE ReleaseVersions SET TimePeriodCoverage = 'AY' WHERE TimePeriodCoverage = 'AYNTP'");
        migrationBuilder.Sql("UPDATE Releases SET TimePeriodCoverage = 'AY' WHERE TimePeriodCoverage = 'AYNTP'");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
