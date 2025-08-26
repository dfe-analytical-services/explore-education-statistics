#nullable disable

using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6429_RemovingAcceptedPublicationAndReleaseInvites : Migration
{
    private const string MigrationId = "20250820103403";

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Delete all Accepted UserPublicationInvites and UserReleaseInvites
        migrationBuilder.SqlFromFile(
            MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES6429_RemovingAcceptedPublicationAndReleaseInvites)}.sql");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
