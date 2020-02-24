using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_UpdateFilterTableType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_UpsertFilter.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.UpsertFilter");
            migrationBuilder.Sql("DROP TYPE dbo.FilterType");
            // Revert FilterType to the version in the previous migration 20200103101609_TableTypes.sql
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_UpdateFilterTableType_Down.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousVersionMigrationId}_Routine_UpsertFilter.sql");
        }
    }
}