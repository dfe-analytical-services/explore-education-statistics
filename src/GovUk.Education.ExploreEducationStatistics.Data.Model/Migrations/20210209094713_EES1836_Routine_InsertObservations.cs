using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1836_Routine_InsertObservations : Migration
    {
        private const string MigrationId = "20210209094713";
        private const string PreviousMigrationId = "20200304145743";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE InsertObservations");
            migrationBuilder.Sql("DROP PROCEDURE InsertObservationFilterItems");
            migrationBuilder.Sql("DROP TYPE ObservationType");
            migrationBuilder.Sql("DROP TYPE ObservationFilterItemType");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationFilterItemType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_InsertObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE InsertObservations");
            migrationBuilder.Sql("DROP TYPE ObservationType");
            migrationBuilder.Sql("DROP TYPE ObservationFilterItemType");
            migrationBuilder.SqlFromFile(PreviousMigrationId, $"{MigrationId}_Type_ObservationType.sql");
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousMigrationId}_Type_ObservationFilterItemType.sql");
            migrationBuilder.SqlFromFile(PreviousMigrationId, $"{PreviousMigrationId}_Routine_InsertObservations.sql");
            migrationBuilder.SqlFromFile(PreviousMigrationId, $"{PreviousMigrationId}_Routine_InsertObservationFilterItems.sql");
        }
    }
}
