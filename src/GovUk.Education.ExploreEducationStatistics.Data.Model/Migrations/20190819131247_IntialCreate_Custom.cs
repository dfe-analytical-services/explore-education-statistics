using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class IntialCreate_Custom : Migration
    {
        private const string _migrationsPath = "Migrations";
        private const string _migrationId = "20190819131247";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Geometry
            ExecuteFile(migrationBuilder, $"{_migrationId}_Geometry.sql");
            // Types
            ExecuteFile(migrationBuilder, $"{_migrationId}_TableTypes.sql");
            // Routines
            ExecuteFile(migrationBuilder, $"{_migrationId}_Routine_FilteredObservations.sql");
            ExecuteFile(migrationBuilder, $"{_migrationId}_Routine_FilteredFootnotes.sql");
            ExecuteFile(migrationBuilder, $"{_migrationId}_Routine_geometry2json.sql");
            // Views
            ExecuteFile(migrationBuilder, $"{_migrationId}_Views.sql");
            // Indexes
            ExecuteFile(migrationBuilder, $"{_migrationId}_Indexes.sql");
            // Data
            ExecuteFile(migrationBuilder, $"{_migrationId}_BoundaryLevelData.sql");
            ExecuteLines(migrationBuilder, $"{_migrationId}_GeometryData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data
            migrationBuilder.Sql("TRUNCATE TABLE dbo.geometry");
            migrationBuilder.Sql("TRUNCATE TABLE dbo.BoundaryLevel");
            // Indexes
            migrationBuilder.Sql("DROP INDEX dbo.NCI_WI_Observation_SubjectId");
            // Views
            migrationBuilder.Sql("DROP VIEW dbo.geojson");
            // Routines
            migrationBuilder.Sql("DROP FUNCTION dbo.geometry2json;");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredFootnotes");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            // Types
            migrationBuilder.Sql("DROP TYPE dbo.TimePeriodListType");
            migrationBuilder.Sql("DROP TYPE dbo.IdListVarcharType");
            migrationBuilder.Sql("DROP TYPE dbo.IdListIntegerType");
            // Geometry
            migrationBuilder.Sql("DROP TABLE dbo.geometry_columns;");
            migrationBuilder.Sql("DROP TABLE dbo.spatial_ref_sys;");
            migrationBuilder.Sql("DROP TABLE dbo.geometry;");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), $"{_migrationsPath}/{filename}");
            migrationBuilder.Sql(File.ReadAllText(file));
        }

        private static void ExecuteLines(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), $"{_migrationsPath}/{filename}");
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                migrationBuilder.Sql(line);
            }
        }
    }
}