using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES701ReplaceChartsWithChart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_Chart",
                table: "ContentBlock",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_Chart = JSON_MODIFY(JSON_QUERY(DataBlock_Charts,'$[0]'), '$.ReleaseId', NULL) WHERE 1=1");

            migrationBuilder.DropColumn(
                name: "DataBlock_Charts",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_Charts",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_Charts = CONCAT('[',JSON_MODIFY(DataBlock_Chart, '$.ReleaseId', ''),']') WHERE 1=1");

            migrationBuilder.DropColumn(
                name: "DataBlock_Chart",
                table: "ContentBlock");
        }
    }
}