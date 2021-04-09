using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2046_Routine_RemoveSoftDeletedSubjects_RevertToUseOldObservationTables : Migration
    {
        private const string MigrationId = "20210408081622";
        private const string PreviousMigrationId = "20210319142402";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // revert the original stored procedure to use just Observation and ObservationFilterItem tables 
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
            
            // add a new stored procedure to use just the ObservationRow and ObservationRowFilterItem tables
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjectsAndObservationRows.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousMigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
            migrationBuilder.Sql("DROP PROCEDURE RemoveSoftDeletedSubjectsAndObservationRows");
        }
    }
}
