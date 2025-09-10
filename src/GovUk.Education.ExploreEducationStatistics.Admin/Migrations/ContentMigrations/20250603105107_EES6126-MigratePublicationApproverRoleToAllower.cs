#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES6126MigratePublicationApproverRoleToAllower : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Migrate all existing Publication `Approver` roles to `Allower`
        migrationBuilder.Sql(@"
                UPDATE upr
                SET upr.Role = 'Allower'
                FROM UserPublicationRoles upr
                WHERE upr.Role = 'Approver'
            ");

        // Migrate all existing Publication `Approver` role invites to `Allower` role invites
        migrationBuilder.Sql(@"
                UPDATE upi
                SET upi.Role = 'Allower'
                FROM UserPublicationInvites upi
                WHERE upi.Role = 'Approver'
            ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
