#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6119MigrateReleaseLeadRolesToContributors : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Migrate all `Lead` roles to `Contributor` roles where the same user DOES NOT already have a `Contributor` role
        // assigned for the same release version
        migrationBuilder.Sql(
            @"
                UPDATE urr
                SET urr.Role = 'Contributor'
                FROM UserReleaseRoles urr
                WHERE urr.Role = 'Lead'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM UserReleaseRoles urr2
                      WHERE urr2.UserId = urr.UserId
                        AND urr2.ReleaseVersionId = urr.ReleaseVersionId
                        AND urr2.Role = 'Contributor'
                  );
            "
        );

        // Delete all `Lead` roles where the same user DOES already have a `Contributor` role assigned for the same
        // release version
        migrationBuilder.Sql(
            @"
                DELETE urr
                FROM UserReleaseRoles urr
                WHERE urr.Role = 'Lead'
                  AND EXISTS (
                      SELECT 1
                      FROM UserReleaseRoles urr2
                      WHERE urr2.UserId = urr.UserId 
                        AND urr2.ReleaseVersionId = urr.ReleaseVersionId 
                        AND urr2.Role = 'Contributor'
                  );
            "
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
