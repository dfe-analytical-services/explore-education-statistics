using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1615UpdateObservationType : Migration
    {
        private const string MigrationId = "20201120142658";
        private const string PreviousInsertObservationsMigrationId = "20200304145743";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservations");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationType");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_ObservationType.sql");

            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousInsertObservationsMigrationId}_Routine_InsertObservations.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.InsertObservations");
            migrationBuilder.Sql("DROP TYPE dbo.ObservationType");

            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Type_Previous_ObservationType.sql");

            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousInsertObservationsMigrationId}_Routine_InsertObservations.sql");
        }
    }
}