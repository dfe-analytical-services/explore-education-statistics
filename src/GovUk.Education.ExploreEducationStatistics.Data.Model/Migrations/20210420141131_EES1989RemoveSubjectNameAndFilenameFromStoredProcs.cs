using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1989RemoveSubjectNameAndFilenameFromStoredProcs : Migration
    {
        private const string MigrationId = "20210420141131";
        private const string PreviousMigrationId = "20210408081622";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjectsAndObservationRows.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousMigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousMigrationId}_Routine_RemoveSoftDeletedSubjectsAndObservationRows.sql");
        }
    }
}
