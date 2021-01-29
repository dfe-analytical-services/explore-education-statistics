using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1830_Routine_RemoveSoftDeletedSubjects : Migration
    {
        private const string MigrationId = "20210129094430";
        private const string PreviousMigrationId = "20201120140950";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousMigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
        }
    }
}
