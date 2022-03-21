using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES3004_UpdateSelectObservationsRoutinesOptimiseFilterItemIds : Migration
    {
        public const string MigrationId = "20220104144102";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{MigrationId}_Routine_SelectObservations.sql");
            
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{MigrationId}_Routine_SelectObservationsByLocationCodes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{EES2778_QueryObservationsByLocationId.MigrationId}_Routine_SelectObservations.sql");
            
            migrationBuilder.SqlFromFile(MigrationsPath, 
                $"{EES2778_QueryObservationsByLocationId.MigrationId}_Routine_SelectObservationsByLocationCodes.sql");
        }
    }
}
