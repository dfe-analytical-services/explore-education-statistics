using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class EES2778_QueryObservationsByLocationId : Migration
    {
        public const string MigrationId = "20211209171502";
        private const string PreviousFilteredObservationsMigrationId = "20210712091202";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new stored procedure for selecting Observations
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_SelectObservations.sql");

            // Rename the existing stored procedure with various location code arguments as SelectObservationsByLocationCodes.
            // This will be maintained to support old Data Blocks that have Location codes rather than id's in their query
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_SelectObservationsByLocationCodes.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredObservations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{PreviousFilteredObservationsMigrationId}_Routine_FilteredObservations.sql");
            migrationBuilder.Sql("DROP PROCEDURE dbo.SelectObservations");
            migrationBuilder.Sql("DROP PROCEDURE dbo.SelectObservationsByLocationCodes");
        }
    }
}
