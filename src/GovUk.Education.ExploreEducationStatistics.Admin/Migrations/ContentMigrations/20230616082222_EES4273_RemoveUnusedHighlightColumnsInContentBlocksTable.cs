using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4273_RemoveUnusedHighlightColumnsInContentBlocksTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataBlock_HighlightDescription",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_HighlightName",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_HighlightDescription",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataBlock_HighlightName",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
