using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2776_AddGeographicLevelToLocation : Migration
    {
        private const string MigrationId = "20211207175748";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeographicLevel",
                table: "Location",
                maxLength: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_GeographicLevel",
                table: "Location",
                column: "GeographicLevel");
            
            // Add a new procedure for conveniently deleting orphaned locations
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DeleteOrphanedLocations.sql");

            // Add new temporary procedure for safely copying Geographic Level from Observations to Locations
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpdateLocationGeographicLevel.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DeleteOrphanedLocations");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpdateLocationGeographicLevel");

            migrationBuilder.DropIndex(
                name: "IX_Location_GeographicLevel",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "GeographicLevel",
                table: "Location");
        }
    }
}
