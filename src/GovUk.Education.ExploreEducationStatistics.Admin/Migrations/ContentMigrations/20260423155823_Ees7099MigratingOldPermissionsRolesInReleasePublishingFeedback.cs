using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class Ees7099MigratingOldPermissionsRolesInReleasePublishingFeedback : Migration
{
    private const string MigrationId = "20260423155823";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Migrate all existing UserPublicationRole entries in the ReleasePublishingFeedback table
        // from the OLD permissions system roles to the corresponding NEW ones:
        // - `Allower` to `Approver`
        // - `Owner` to `Drafter`
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_{nameof(Ees7099MigratingOldPermissionsRolesInReleasePublishingFeedback)}.sql"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
