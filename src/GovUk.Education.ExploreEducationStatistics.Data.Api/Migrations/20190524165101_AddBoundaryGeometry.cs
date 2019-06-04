using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddBoundaryGeometry : Migration
    {
        private readonly string _migrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ExecuteFile(migrationBuilder, _migrationsPath + "20190524165101_Create_geometry_columns.sql");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190524165101_Create_spatial_ref_sys.sql");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190524165101_Create_geometry.sql");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190524165101_AddGeoJsonFunction.sql");
            ExecuteFile(migrationBuilder, _migrationsPath + "20190524165101_AddGeoJsonView.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW dbo.geojson");
            migrationBuilder.Sql("DROP FUNCTION dbo.geometry2json;");
            migrationBuilder.Sql("DROP TABLE dbo.geometry;");
            migrationBuilder.Sql("DROP TABLE dbo.spatial_ref_sys;");
            migrationBuilder.Sql("DROP TABLE dbo.geometry_columns;");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
