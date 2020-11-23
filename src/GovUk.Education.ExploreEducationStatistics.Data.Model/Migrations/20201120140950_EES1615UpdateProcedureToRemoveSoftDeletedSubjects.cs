using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1615UpdateProcedureToRemoveSoftDeletedSubjects : Migration
    {
        private const string MigrationId = "20201120140950";
        private const string PreviousVersionMigrationId = "20200910135857";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the version in the previous migration 20200910135857_Routine_RemoveSoftDeletedSubjects.sql
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousVersionMigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
        }
    }
}