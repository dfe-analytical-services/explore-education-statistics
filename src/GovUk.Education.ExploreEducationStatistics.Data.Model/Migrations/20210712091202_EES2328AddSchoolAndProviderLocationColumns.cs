using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2328AddSchoolAndProviderLocationColumns : Migration
    {
        private const string MigrationId = "20210712091202";
        private const string PreviousFilteredObservationsMigrationId = "20210512112804";
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provider_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "School_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "School_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_Provider_Code",
                table: "Location",
                column: "Provider_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_School_Code",
                table: "Location",
                column: "School_Code");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Location_Provider_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_School_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Provider_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Provider_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "School_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "School_Name",
                table: "Location");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousFilteredObservationsMigrationId}_Routine_FilteredObservations.sql");
        }
    }
}
