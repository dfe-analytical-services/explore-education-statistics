using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES344ReplaceTablesWithTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_Table",
                table: "ContentBlock",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_Table = JSON_QUERY(DataBlock_Tables,'$[0]') WHERE 1=1");

            migrationBuilder.DropColumn(
                name: "DataBlock_Tables",
                table: "ContentBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataBlock_Tables",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE dbo.ContentBlock SET DataBlock_Tables = CONCAT('[',JSON_QUERY(DataBlock_Table, '$'),']') WHERE 1=1");

            migrationBuilder.DropColumn(
                name: "DataBlock_Table",
                table: "ContentBlock");
        }
    }
}