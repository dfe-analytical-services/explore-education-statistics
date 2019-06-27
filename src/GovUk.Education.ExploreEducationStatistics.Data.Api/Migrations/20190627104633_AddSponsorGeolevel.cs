using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddSponsorGeolevel : Migration
    {
        private const string _migrationsPath = "Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Mat_Name",
                table: "Location",
                newName: "MultiAcademyTrust_Name");

            migrationBuilder.RenameColumn(
                name: "Mat_Code",
                table: "Location",
                newName: "MultiAcademyTrust_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Location_Mat_Code",
                table: "Location",
                newName: "IX_Location_MultiAcademyTrust_Code");

            migrationBuilder.AddColumn<string>(
                name: "Sponsor_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sponsor_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_Sponsor_Code",
                table: "Location",
                column: "Sponsor_Code");

            ExecuteFile(migrationBuilder, _migrationsPath + "20190627104633_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the previous version of the stored proc
            ExecuteFile(migrationBuilder, _migrationsPath + "20190613160151_FilteredObservations.sql");

            migrationBuilder.DropIndex(
                name: "IX_Location_Sponsor_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Sponsor_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Sponsor_Name",
                table: "Location");

            migrationBuilder.RenameColumn(
                name: "MultiAcademyTrust_Name",
                table: "Location",
                newName: "Mat_Name");

            migrationBuilder.RenameColumn(
                name: "MultiAcademyTrust_Code",
                table: "Location",
                newName: "Mat_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Location_MultiAcademyTrust_Code",
                table: "Location",
                newName: "IX_Location_Mat_Code");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}