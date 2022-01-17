using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class InitialCreate_Custom : Migration
    {
        public const string MigrationId = "20210512112804";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Geometry
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Table_Geometry.sql");
            // Types
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableTypes.sql");
            // Routines
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DropAndCreateRelease.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredFootnotes.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservationRows.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_FilteredObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_geometry2json.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_GetFilteredObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservationFilterItems.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservationRows.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RebuildIndexes.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_RemoveSoftDeletedSubjectsAndObservationRows.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertLocation.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertPublication.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertTheme.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_UpsertTopic.sql");
            // Views
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_View_geojson.sql");
            // Indexes
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Indexes.sql");
            // Data
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Data_BoundaryLevel.sql");
            migrationBuilder.SqlFromFileByLine(MigrationsPath, $"{MigrationId}_Data_Geometry.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data
            migrationBuilder.Sql("ALTER TABLE dbo.geometry DROP CONSTRAINT geometry_BoundaryLevel_Id_fk");
            migrationBuilder.Sql("TRUNCATE TABLE dbo.geometry");
            migrationBuilder.Sql("TRUNCATE TABLE dbo.BoundaryLevel");
            // Indexes
            migrationBuilder.Sql("DROP INDEX NCI_WI_Observation_SubjectId on Observation");
            // Views
            migrationBuilder.Sql("DROP VIEW dbo.geojson");
            // Routines
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredFootnotes");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservationRows");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
            migrationBuilder.Sql("DROP FUNCTION dbo.geometry2json");
            migrationBuilder.Sql("DROP PROCEDURE dbo.GetFilteredObservations");
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservationFilterItems");
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservationRows");
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservations");
            migrationBuilder.Sql("DROP PROCEDURE dbo.RebuildIndexes");
            migrationBuilder.Sql("DROP PROCEDURE dbo.RemoveSoftDeletedSubjects");
            migrationBuilder.Sql("DROP PROCEDURE dbo.RemoveSoftDeletedSubjectsAndObservationRows");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertLocation");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertPublication");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertTheme");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertTopic");
            // Types
            migrationBuilder.Sql("DROP TYPE dbo.FilterTableType");
            migrationBuilder.Sql("DROP TYPE dbo.FootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.IdListGuidType");
            migrationBuilder.Sql("DROP TYPE dbo.IdListIntegerType");
            migrationBuilder.Sql("DROP TYPE dbo.IdListVarcharType");
            migrationBuilder.Sql("DROP TYPE dbo.LocationType");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationFilterItemType");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationRowFilterItemType");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationRowType");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationType");
            migrationBuilder.Sql("DROP TYPE dbo.PublicationType");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            migrationBuilder.Sql("DROP TYPE dbo.ThemeType");
            migrationBuilder.Sql("DROP TYPE dbo.TimePeriodListType");
            migrationBuilder.Sql("DROP TYPE dbo.TopicType");
            // Geometry
            migrationBuilder.Sql("DROP TABLE dbo.geometry_columns;");
            migrationBuilder.Sql("DROP TABLE dbo.spatial_ref_sys;");
            migrationBuilder.Sql("DROP TABLE dbo.geometry;");
        }
    }
}
