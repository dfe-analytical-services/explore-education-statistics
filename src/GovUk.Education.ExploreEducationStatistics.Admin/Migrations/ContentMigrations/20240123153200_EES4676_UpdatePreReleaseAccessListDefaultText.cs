using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

public partial class EES4676_UpdatePreReleaseAccessListDefaultText : Migration
{
    private const string MigrationId = "20240123153200";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(
            ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES4676_UpdatePreReleaseAccessListDefaultText)}.sql");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
