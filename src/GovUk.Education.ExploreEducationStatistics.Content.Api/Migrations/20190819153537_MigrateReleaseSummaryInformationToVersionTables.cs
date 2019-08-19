using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Migrations
{
    public partial class MigrateReleaseSummaryInformationToVersionTables : Migration
    {
        private const string MigrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, MigrationsPath + "20190819153537_MigrateReleaseSummaryInformationToVersionTables.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
