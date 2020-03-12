using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class UseStoredProceduresForImporter : Migration
    {
        private const string MigrationsPath = "Migrations";
        private const string MigrationId = "20200304145743";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_TableTypes.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservations.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservationFilterItems.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservations");
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservationFilterItems");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationType");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationFilterItemType");
        }
    }
}
