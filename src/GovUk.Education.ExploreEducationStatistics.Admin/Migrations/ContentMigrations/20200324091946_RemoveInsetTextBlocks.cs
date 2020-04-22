using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class RemoveInsetTextBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InsetTextBlock_Body",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "InsetTextBlock_Heading",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsetTextBlock_Body",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsetTextBlock_Heading",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
