using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddProvider : Migration
    {
        private const string _migrationsPath = "Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Location_Provider_Code",
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

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "School",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "School",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderUrn",
                table: "Observation",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Provider",
                columns: table => new
                {
                    Urn = table.Column<string>(nullable: false),
                    Ukprn = table.Column<string>(nullable: true),
                    Upin = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Provider", x => x.Urn); });

            migrationBuilder.CreateIndex(
                name: "IX_Observation_ProviderUrn",
                table: "Observation",
                column: "ProviderUrn");

            migrationBuilder.AddForeignKey(
                name: "FK_Observation_Provider_ProviderUrn",
                table: "Observation",
                column: "ProviderUrn",
                principalTable: "Provider",
                principalColumn: "Urn",
                onDelete: ReferentialAction.Restrict);

            ExecuteFile(migrationBuilder, _migrationsPath + "20190613160151_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the previous version of the stored proc
            ExecuteFile(migrationBuilder, _migrationsPath + "20190612155940_FilteredObservations.sql");

            migrationBuilder.DropForeignKey(
                name: "FK_Observation_Provider_ProviderUrn",
                table: "Observation");

            migrationBuilder.DropTable(
                name: "Provider");

            migrationBuilder.DropIndex(
                name: "IX_Observation_ProviderUrn",
                table: "Observation");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "School");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "School");

            migrationBuilder.DropColumn(
                name: "ProviderUrn",
                table: "Observation");

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

            migrationBuilder.CreateIndex(
                name: "IX_Location_Provider_Code",
                table: "Location",
                column: "Provider_Code");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}