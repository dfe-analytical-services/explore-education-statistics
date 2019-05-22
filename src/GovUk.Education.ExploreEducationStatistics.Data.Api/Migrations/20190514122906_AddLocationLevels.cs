using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage] 
    public partial class AddLocationLevels : Migration
    {
        private readonly string _migrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Institution_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Institution_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalEnterprisePartnership_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalEnterprisePartnership_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mat_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mat_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MayoralCombinedAuthority_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MayoralCombinedAuthority_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpportunityArea_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpportunityArea_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParliamentaryConstituency_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParliamentaryConstituency_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider_Ukprn",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider_Upin",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ward_Code",
                table: "Location",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ward_Name",
                table: "Location",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_Institution_Code",
                table: "Location",
                column: "Institution_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocalEnterprisePartnership_Code",
                table: "Location",
                column: "LocalEnterprisePartnership_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Mat_Code",
                table: "Location",
                column: "Mat_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_MayoralCombinedAuthority_Code",
                table: "Location",
                column: "MayoralCombinedAuthority_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_OpportunityArea_Code",
                table: "Location",
                column: "OpportunityArea_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ParliamentaryConstituency_Code",
                table: "Location",
                column: "ParliamentaryConstituency_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Provider_Code",
                table: "Location",
                column: "Provider_Code");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Ward_Code",
                table: "Location",
                column: "Ward_Code");
            
            // Add this version of the stored proc
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190514122906_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Location_Institution_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_LocalEnterprisePartnership_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_Mat_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_MayoralCombinedAuthority_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_OpportunityArea_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_ParliamentaryConstituency_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_Provider_Code",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_Ward_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Institution_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Institution_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "LocalEnterprisePartnership_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "LocalEnterprisePartnership_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Mat_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Mat_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "MayoralCombinedAuthority_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "MayoralCombinedAuthority_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "OpportunityArea_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "OpportunityArea_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "ParliamentaryConstituency_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "ParliamentaryConstituency_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Provider_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Provider_Name",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Provider_Ukprn",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Provider_Upin",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Ward_Code",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "Ward_Name",
                table: "Location");
            
            // Revert to the previous version of the stored proc
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190507122936_FilteredObservations.sql");
        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
