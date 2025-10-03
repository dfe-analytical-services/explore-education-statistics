using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES5368_UpdateRebuildIndexesSPToWriteRunDataToDB : Migration
{
    private const string PreviousRebuildIndexesMigrationId = EES5205_UpdateRebuildIndexesStoredProc.MigrationId;
    internal const string MigrationId = "20240802123234";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath, $"{MigrationId}_Routine_RebuildIndexes.sql");
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
