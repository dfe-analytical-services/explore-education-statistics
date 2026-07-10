using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees7173MoveCreationOfIndexRebuildTrackingTablesOutOfStoredProcedure : Migration
    {
        private const string MigrationId = "20260709194542";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_{nameof(Ees7173MoveCreationOfIndexRebuildTrackingTablesOutOfStoredProcedure)}.sql"
            );

            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_Routine_RebuildIndexes.sql"
            );

            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON __Log_RebuildIndexes TO [datafactory]");
            migrationBuilder.Sql("GRANT SELECT, INSERT, UPDATE ON __Log_RebuildIndexesAlterIndexes TO [datafactory]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
