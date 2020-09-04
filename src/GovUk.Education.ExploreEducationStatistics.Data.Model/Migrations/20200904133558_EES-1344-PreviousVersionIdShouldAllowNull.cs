using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1344PreviousVersionIdShouldAllowNull : Migration
    {
        private const string MigrationId = "20200904133558";
        private const string PreviousReleaseTypeMigrationId = "20200713151302";
        private const string PreviousDropAndCreateReleaseMigrationId = "20200715112018";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ReleaseType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousDropAndCreateReleaseMigrationId}_Routine_DropAndCreateRelease.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousReleaseTypeMigrationId}_Type_ReleaseType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousDropAndCreateReleaseMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}
