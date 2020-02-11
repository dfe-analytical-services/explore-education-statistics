using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class RemoveRedundantProcs : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200211122058";
        private const string OrigMigrationId = "20200103101609";

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Types
            ExecuteFile(migrationBuilder, $"{MigrationId}_RemoveRedundantProcs_down.sql");
            // Routines
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertFilter.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertFilterFootnote.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertFilterGroup.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertFilterGroupFootnote.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertFilterItem.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertFilterItemFootnote.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertIndicator.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertIndicatorFootnote.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertIndicatorGroup.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertRelease.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertSubject.sql");
            ExecuteFile(migrationBuilder, $"{OrigMigrationId}_Routine_UpsertSubjectFootnote.sql");
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Routines
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilter");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterFootnote");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterGroup");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterGroupFootnote");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterItem");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilterItemFootnote");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertIndicator");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertIndicatorFootnote");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertIndicatorGroup");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertSubject");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertSubjectFootnote");
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertRelease");
            // Types
            migrationBuilder.Sql("DROP TYPE dbo.FilterType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterGroupType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterGroupFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterItemType");
            migrationBuilder.Sql("DROP TYPE dbo.FilterItemFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.IndicatorType");
            migrationBuilder.Sql("DROP TYPE dbo.IndicatorFootnoteType");
            migrationBuilder.Sql("DROP TYPE dbo.IndicatorGroupType");
            migrationBuilder.Sql("DROP TYPE dbo.SubjectType");
            migrationBuilder.Sql("DROP TYPE dbo.SubjectFootnoteType");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{MigrationsPath}{Path.DirectorySeparatorChar}{filename}");

            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
