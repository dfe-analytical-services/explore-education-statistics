using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2999_DropSelectObservationsRoutines : Migration
    {
        private const string PreviousMigrationId =
            EES3004_UpdateSelectObservationsRoutinesOptimiseFilterItemIds.MigrationId;
            
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE SelectObservations");
            migrationBuilder.Sql("DROP PROCEDURE SelectObservationsByLocationCodes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{PreviousMigrationId}_Routine_SelectObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{PreviousMigrationId}_Routine_SelectObservationsByLocationCodes.sql");
        }
    }
}
