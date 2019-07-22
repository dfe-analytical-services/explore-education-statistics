using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class RestrictByGeographicLevel : Migration
    {
        private const string _migrationsPath = "Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, _migrationsPath + "20190717083713_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to the previous version of the stored proc
            ExecuteFile(migrationBuilder, _migrationsPath + "20190709121039_FilteredObservations.sql");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}