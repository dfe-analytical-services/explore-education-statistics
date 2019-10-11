using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class UpdateFilteredObservationsStoredProc : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20191008162516";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_FilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            const string previousVersionMigrationId = "20190819131247";
            ExecuteFile(migrationBuilder, $"{previousVersionMigrationId}_Routine_FilteredObservations.sql");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), $"{MigrationsPath}/{filename}");
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}