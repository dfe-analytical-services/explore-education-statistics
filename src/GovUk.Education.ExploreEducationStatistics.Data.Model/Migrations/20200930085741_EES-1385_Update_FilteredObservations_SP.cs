using Microsoft.EntityFrameworkCore.Migrations;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1385_Update_FilteredObservations_SP : Migration
    {
        private const string MigrationId = "20200930085741";
        private const string PreviousInsertObservationFilterItemsMigrationId = "20200304145743";
        private const string PreviousFilteredObservationsMigrationId = "20200308154804";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservationFilterItems");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationFilterItemType");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationFilterItemType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousInsertObservationFilterItemsMigrationId}_Routine_InsertObservationFilterItems.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_FilterTableType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_GetFilteredObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TYPE ObservationFilterItemType AS table( ObservationId uniqueidentifier NOT NULL, FilterItemId  uniqueidentifier NOT NULL)");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousInsertObservationFilterItemsMigrationId}_Routine_InsertObservationFilterItems.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.GetFilteredObservations");
            migrationBuilder.Sql("DROP TYPE dbo.FilterTableType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousFilteredObservationsMigrationId}_Routine_FilteredObservations.sql");
        }
    }
}
