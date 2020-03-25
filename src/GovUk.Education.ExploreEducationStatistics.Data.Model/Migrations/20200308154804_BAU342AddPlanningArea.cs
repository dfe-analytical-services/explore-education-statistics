using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class BAU342AddPlanningArea : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200308154804";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanningArea_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanningArea_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_PlanningArea_Code",
                table: "Location",
                column: "PlanningArea_Code");
            
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertLocation");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            migrationBuilder.Sql("DROP TYPE dbo.LocationType");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertLocation.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Location_PlanningArea_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PlanningArea_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "PlanningArea_Name",
                table: "Location");
            
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertLocation");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            migrationBuilder.Sql("DROP TYPE dbo.LocationType");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_Previous_LocationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, "20190819131247_Routine_FilteredObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, "20200103101609_Routine_UpsertLocation.sql");
        }
    }
}
