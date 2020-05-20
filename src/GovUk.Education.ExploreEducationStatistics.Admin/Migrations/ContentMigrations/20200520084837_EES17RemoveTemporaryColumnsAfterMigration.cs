using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES17RemoveTemporaryColumnsAfterMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataBlock_EES17Request",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_EES17Tables",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_EES17Request",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataBlock_EES17Tables",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
