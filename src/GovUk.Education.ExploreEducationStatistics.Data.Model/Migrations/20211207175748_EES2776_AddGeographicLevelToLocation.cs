using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2776_AddGeographicLevelToLocation : Migration
    {
        public const string MigrationId = "20211207175748";
        private const string PreviousLocationTypeMigrationId = E2328UpdateLocationTypeAndUpsertLocation.MigrationId;
        private const string PreviousUpsertLocationMigrationId = E2328UpdateLocationTypeAndUpsertLocation.MigrationId;

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

            // Update LocationType
            migrationBuilder.Sql("DROP PROCEDURE UpsertLocation");
            migrationBuilder.Sql("DROP TYPE LocationType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableType_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertLocation.sql");

            // Add new temporary procedure for safely copying Geographic Level from Observations to Locations
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpdateLocationGeographicLevel.sql");

            // Add a new procedure for conveniently deleting orphaned locations
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DeleteOrphanedLocations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.DeleteOrphanedLocations");

            migrationBuilder.Sql("DROP PROCEDURE dbo.UpdateLocationGeographicLevel");

            migrationBuilder.Sql("DROP PROCEDURE UpsertLocation");
            migrationBuilder.Sql("DROP TYPE LocationType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousLocationTypeMigrationId}_TableType_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousUpsertLocationMigrationId}_Routine_UpsertLocation.sql");

            migrationBuilder.DropIndex(
                name: "IX_Location_GeographicLevel",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "GeographicLevel",
                table: "Location");
        }
    }
}
