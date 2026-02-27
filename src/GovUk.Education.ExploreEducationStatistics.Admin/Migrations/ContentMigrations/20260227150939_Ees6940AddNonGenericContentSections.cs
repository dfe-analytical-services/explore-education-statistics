using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class Ees6940AddNonGenericContentSections : Migration
{
    private const string MigrationId = "20260227150939";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Insert content sections for existing release versions
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_{nameof(Ees6940AddNonGenericContentSections)}.sql"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
