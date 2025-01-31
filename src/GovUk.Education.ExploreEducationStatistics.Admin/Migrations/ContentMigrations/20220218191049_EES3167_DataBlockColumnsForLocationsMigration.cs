using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3167_DataBlockColumnsForLocationsMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_ChartsMigrated",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DataBlock_LocationsMigrated",
                table: "ContentBlock",
                type: "bit",
                nullable: true,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DataBlock_QueryMigrated",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataBlock_TableMigrated",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataBlock_ChartsMigrated",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_LocationsMigrated",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_QueryMigrated",
                table: "ContentBlock");

            migrationBuilder.DropColumn(
                name: "DataBlock_TableMigrated",
                table: "ContentBlock");
        }
    }
}
