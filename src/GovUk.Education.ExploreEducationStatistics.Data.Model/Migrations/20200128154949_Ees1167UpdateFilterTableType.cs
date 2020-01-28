using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class Ees1167UpdateFilterTableType : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200128154949";
        const string PreviousVersionMigrationId = "20200103101609";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilter");
            migrationBuilder.Sql("DROP TYPE dbo.FilterType");
            ExecuteFile(migrationBuilder, $"{MigrationId}_UpdateFilterTableType.sql");
            ExecuteFile(migrationBuilder, $"{PreviousVersionMigrationId}_Routine_UpsertFilter.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilter");
            migrationBuilder.Sql("DROP TYPE dbo.FilterType");
            // Revert FilterType to the version in the previous migration 20200103101609_TableTypes.sql
            ExecuteFile(migrationBuilder, $"{MigrationId}_UpdateFilterTableType_Down.sql");
            ExecuteFile(migrationBuilder, $"{PreviousVersionMigrationId}_Routine_UpsertFilter.sql");
        }

        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                $"{MigrationsPath}{Path.DirectorySeparatorChar}{filename}");

            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}