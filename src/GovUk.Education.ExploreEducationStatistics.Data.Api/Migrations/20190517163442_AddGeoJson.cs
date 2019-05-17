using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddGeoJson : Migration
    {
        private readonly string _migrationsPath = "Migrations/";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, _migrationsPath + "20190517163442_AddGeoJsonFunction.sql");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190517163442_AddGeoJsonView.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.geojson");
            migrationBuilder.Sql("DROP FUNCTION dbo.geometry2json;");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
