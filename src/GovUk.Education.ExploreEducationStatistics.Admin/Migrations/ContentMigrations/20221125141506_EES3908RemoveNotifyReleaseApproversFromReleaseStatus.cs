using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3908RemoveNotifyReleaseApproversFromReleaseStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyReleaseApprovers",
                table: "ReleaseStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyReleaseApprovers",
                table: "ReleaseStatus",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
