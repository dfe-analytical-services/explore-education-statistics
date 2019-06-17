using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddRscRegion : Migration
    {
        private const string _migrationsPath = "Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RscRegion_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_RscRegion_Code",
                table: "Location",
                column: "RscRegion_Code");
            
            ExecuteFile(migrationBuilder, _migrationsPath + "20190612155940_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the previous version of the stored proc
            ExecuteFile(migrationBuilder, _migrationsPath + "20190514122906_FilteredObservations.sql");
            
            migrationBuilder.DropIndex(
                name: "IX_Location_RscRegion_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "RscRegion_Code",
                table: "Location");
        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
