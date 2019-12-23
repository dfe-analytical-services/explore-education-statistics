using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class IntialCreate_Custom : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20190819131247";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Geometry
            ExecuteFile(migrationBuilder, $"{MigrationId}_Geometry.sql");
            // Types
            ExecuteFile(migrationBuilder, $"{MigrationId}_TableTypes.sql");
            // Routines
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_FilteredObservations.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_FilteredFootnotes.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_geometry2json.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertTheme.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertTopic.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertPublication.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertRelease.sql");

            // Views
            ExecuteFile(migrationBuilder, $"{MigrationId}_Views.sql");
            // Indexes
            ExecuteFile(migrationBuilder, $"{MigrationId}_Indexes.sql");
            // Data
            ExecuteFile(migrationBuilder, $"{MigrationId}_BoundaryLevelData.sql");
            ExecuteLines(migrationBuilder, $"{MigrationId}_GeometryData.sql");
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
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertTheme");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertTopic");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertPublication");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertRelease");
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
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{MigrationsPath}{Path.DirectorySeparatorChar}{filename}");
            
            migrationBuilder.Sql(File.ReadAllText(file));
        }

        private static void ExecuteLines(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{MigrationsPath}{Path.DirectorySeparatorChar}{filename}");
            
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                migrationBuilder.Sql(line);
            }
        }
    }
}