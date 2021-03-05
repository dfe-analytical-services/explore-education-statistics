using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1921AddEnglishDevolvedAreaGeographicLevel : Migration
    {
        private const string MigrationId = "20210305120517";
        private const string PreviousFilteredObservationsMigrationId = "20210129135020";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishDevolvedArea_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishDevolvedArea_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_EnglishDevolvedArea_Code",
                table: "Location",
                column: "EnglishDevolvedArea_Code");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousFilteredObservationsMigrationId}_Routine_FilteredObservations.sql");

            migrationBuilder.DropIndex(
                name: "IX_Location_EnglishDevolvedArea_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "EnglishDevolvedArea_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "EnglishDevolvedArea_Name",
                table: "Location");
        }
    }
}
