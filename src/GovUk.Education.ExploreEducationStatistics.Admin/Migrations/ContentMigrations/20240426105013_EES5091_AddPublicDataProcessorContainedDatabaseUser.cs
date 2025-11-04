#nullable disable

using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES5091_AddPublicDataProcessorContainedDatabaseUser : Migration
{
    private const string MigrationId = "20240426105013";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_EES5091_AddPublicDataProcessorContainedDatabaseUser.sql"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
