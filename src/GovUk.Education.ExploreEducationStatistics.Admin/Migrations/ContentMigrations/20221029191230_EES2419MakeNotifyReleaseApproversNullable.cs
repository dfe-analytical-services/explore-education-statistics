using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2419MakeNotifyReleaseApproversNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyReleaseApprovers",
                table: "Releases");

            migrationBuilder.AlterColumn<bool>(
                name: "NotifyReleaseApprovers",
                table: "ReleaseStatus",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "NotifyReleaseApprovers",
                table: "ReleaseStatus",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyReleaseApprovers",
                table: "Releases",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
