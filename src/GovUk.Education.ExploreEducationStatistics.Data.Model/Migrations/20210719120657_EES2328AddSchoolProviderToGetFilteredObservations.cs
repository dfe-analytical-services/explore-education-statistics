using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2328AddSchoolProviderToGetFilteredObservations : Migration
    {
        private const string MigrationId = "20210719120657";
        private const string PreviousGetFilteredObservationsMigrationId = "20210512112804";
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_GetFilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousGetFilteredObservationsMigrationId}_Routine_GetFilteredObservations.sql");
        }
    }
}
