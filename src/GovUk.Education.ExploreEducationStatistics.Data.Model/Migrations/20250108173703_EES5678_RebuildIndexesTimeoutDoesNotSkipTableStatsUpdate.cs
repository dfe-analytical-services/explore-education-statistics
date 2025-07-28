using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
public partial class EES5678_RebuildIndexesTimeoutDoesNotSkipTableStatsUpdate : Migration
{
    private const string PreviousRebuildIndexesMigrationId = EES5368_UpdateRebuildIndexesSPToWriteRunDataToDB.MigrationId;
    internal const string MigrationId = "20250108173703";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS [dbo].[__Log_RebuildIndexesUpdateStatistics]");

        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{MigrationId}_TableType_ModifiedTablesType.sql");

        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{MigrationId}_Routine_UpdateStatistics.sql");

        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{MigrationId}_Routine_RebuildIndexes.sql");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP PROCEDURE [dbo].[UpdateStatistics]");

        migrationBuilder.Sql("DROP TYPE [dbo].[ModifiedTablesType]");

        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{PreviousRebuildIndexesMigrationId}_Routine_RebuildIndexes.sql");
    }
}
