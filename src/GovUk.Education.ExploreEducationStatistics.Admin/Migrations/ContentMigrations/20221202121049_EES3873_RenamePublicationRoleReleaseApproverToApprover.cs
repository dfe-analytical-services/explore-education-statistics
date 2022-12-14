using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3873_RenamePublicationRoleReleaseApproverToApprover : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE UserPublicationInvites SET Role = 'Approver' WHERE Role = 'ReleaseApprover'");
            migrationBuilder.Sql("UPDATE UserPublicationRoles SET Role = 'Approver' WHERE Role = 'ReleaseApprover'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE UserPublicationInvites SET Role = 'ReleaseApprover' WHERE Role = 'Approver'");
            migrationBuilder.Sql("UPDATE UserPublicationRoles SET Role = 'ReleaseApprover' WHERE Role = 'Approver'");
        }
    }
}
