using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class EES6434_ImproveLoggingInRemoveSoftDeletedSubjectsSP : Migration
{
    internal const string MigrationId = "20250818152801";
    private const string PreviousRemoveSoftDeletedSubjectsMigrationId =
        InitialCreate_Custom.MigrationId;

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(
            MigrationConstants.MigrationsPath,
            $"{MigrationId}_Routine_RemoveSoftDeletedSubjects.sql"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(
            MigrationConstants.MigrationsPath,
            $"{PreviousRemoveSoftDeletedSubjectsMigrationId}_Routine_RemoveSoftDeletedSubjects.sql"
        );
    }
}
