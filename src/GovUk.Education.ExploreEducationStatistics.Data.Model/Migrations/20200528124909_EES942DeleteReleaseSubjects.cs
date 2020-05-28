using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES942DeleteReleaseSubjects : Migration
    {
        private const string MigrationId = "20200528124909";
        private const string PreviousVersionMigrationId = "20200303144410";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DropAndCreateRelease.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the version in the previous migration 20200303144410_Routine_DropAndCreateRelease.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}