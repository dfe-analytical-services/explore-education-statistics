using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

[ExcludeFromCodeCoverage]
public partial class EES4480_AlterStoredProcRebuildIndexes : Migration
{
    private const string PreviousRebuildIndexesMigrationId = EES3993AlterStoredProcRebuildIndexes.MigrationId;
    private const string MigrationId = "20230816151226";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{MigrationId}_Routine_RebuildIndexes.sql");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(MigrationConstants.MigrationsPath,
            $"{PreviousRebuildIndexesMigrationId}_Routine_RebuildIndexes.sql");
    }
}
