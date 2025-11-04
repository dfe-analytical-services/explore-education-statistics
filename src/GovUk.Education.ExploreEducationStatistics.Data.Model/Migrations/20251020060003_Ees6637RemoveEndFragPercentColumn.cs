using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees6637RemoveEndFragPercentColumn : Migration
    {
        private const string PreviousRebuildIndexesMigrationId =
            EES5678_RebuildIndexesTimeoutDoesNotSkipTableStatsUpdate.MigrationId;
        internal const string MigrationId = "20251020060003";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 
                           FROM sys.columns 
                           WHERE Name = N'EndFragPercent' 
                           AND Object_ID = OBJECT_ID(N'dbo.__Log_RebuildIndexesAlterIndexes', 'U'))
                BEGIN
                    ALTER TABLE dbo.__Log_RebuildIndexesAlterIndexes
                    DROP COLUMN EndFragPercent;
                END
                """
            );
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{MigrationId}_Routine_RebuildIndexes.sql"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1
                           FROM sys.columns
                           WHERE Name = N'EndFragPercent'
                           AND Object_ID = OBJECT_ID(N'dbo.__Log_RebuildIndexesAlterIndexes', 'U'))
                BEGIN
                    ALTER TABLE dbo.__Log_RebuildIndexesAlterIndexes
                    ADD EndFragPercent FLOAT NULL;
                END
                """
            );
            migrationBuilder.SqlFromFile(
                MigrationConstants.MigrationsPath,
                $"{PreviousRebuildIndexesMigrationId}_Routine_RebuildIndexes.sql"
            );
        }
    }
}
