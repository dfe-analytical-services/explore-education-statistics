using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees6696Routine_RebuildIndexes : Migration
    {
        private const string PreviousRebuildIndexesMigrationId = Ees6637RemoveEndFragPercentColumn.MigrationId;
        internal const string MigrationId = "20251112113517";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_Routine_RebuildIndexes.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{PreviousRebuildIndexesMigrationId}_Routine_RebuildIndexes.sql"
            );
        }
    }
}
