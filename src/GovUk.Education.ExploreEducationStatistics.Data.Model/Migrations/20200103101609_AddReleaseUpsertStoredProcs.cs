using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;
using System.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class AddReleaseUpsertStoredProcs : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200103101609";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Types
            ExecuteFile(migrationBuilder, $"{MigrationId}_TableTypes.sql");
            // Routines
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFilter.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFilterFootnote.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFilterGroup.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFilterGroupFootnote.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFilterItem.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFilterItemFootnote.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertFootnote.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertIndicator.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertIndicatorFootnote.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertIndicatorGroup.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertLocation.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertPublication.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertRelease.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertSubject.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertSubjectFootnote.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertTheme.sql");
            ExecuteFile(migrationBuilder, $"{MigrationId}_Routine_UpsertTopic.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Routines
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilter.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterFootnote.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterGroup.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterGroupFootnote.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterItem.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterItemFootnote.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFootnote.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertIndicator.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertIndicatorFootnote.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertIndicatorGroup.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertLocation.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertPublication.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertRelease.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertSubject.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertSubjectFootnote.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertTheme.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertTopic.sql");

            // Types
            migrationBuilder.Sql("DROP TYPE dbo.FilterType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterGroupType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterGroupFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterItemType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterItemFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.FootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.IndicatorType");
            migrationBuilder.Sql("DROP TYPE dbo.IndicatorFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.IndicatorGroupType");
            migrationBuilder.Sql("DROP TYPE dbo.LocationType");
            migrationBuilder.Sql("DROP TYPE dbo.PublicationType");
            migrationBuilder.Sql("DROP TYPE dbo.ReleaseType");
            migrationBuilder.Sql("DROP TYPE dbo.SubjectType");
            migrationBuilder.Sql("DROP TYPE dbo.SubjectFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.ThemeType");
            migrationBuilder.Sql("DROP TYPE dbo.TopicType");
        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{MigrationsPath}{Path.DirectorySeparatorChar}{filename}");
            
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}

