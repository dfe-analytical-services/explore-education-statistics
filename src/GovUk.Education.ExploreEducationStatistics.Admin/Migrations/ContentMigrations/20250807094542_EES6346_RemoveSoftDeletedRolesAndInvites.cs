#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6346_RemoveSoftDeletedRolesAndInvites : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Remove all historic soft-deleted Publication/Release Roles and Invites
        migrationBuilder.Sql(@"
            DELETE FROM dbo.UserPublicationRoles WHERE Deleted IS NOT NULL;
            DELETE FROM dbo.UserReleaseRoles WHERE Deleted IS NOT NULL OR SoftDeleted = 1;
            DELETE FROM dbo.UserReleaseInvites WHERE SoftDeleted = 1;
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
