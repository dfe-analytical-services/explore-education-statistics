using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1836_Routine_InsertObservationRows : Migration
    {
        private const string MigrationId = "20210302010000";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationRowType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationRowFilterItemType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservationRows.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE InsertObservationRows");
            migrationBuilder.Sql("DROP TYPE ObservationRowType");
            migrationBuilder.Sql("DROP TYPE ObservationRowFilterItemType");
        }
    }
}
