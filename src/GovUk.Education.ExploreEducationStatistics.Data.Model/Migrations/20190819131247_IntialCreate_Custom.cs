using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class IntialCreate_Custom : Migration
    {
        private const string MigrationId = "20190819131247";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Geometry
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Geometry.sql");
            // Types
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableTypes.sql");
            // Routines
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredFootnotes.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_geometry2json.sql");
            // Views
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Views.sql");
            // Indexes
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Indexes.sql");
            // Data
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_BoundaryLevelData.sql");
            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_GeometryData.sql");
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
    }
}