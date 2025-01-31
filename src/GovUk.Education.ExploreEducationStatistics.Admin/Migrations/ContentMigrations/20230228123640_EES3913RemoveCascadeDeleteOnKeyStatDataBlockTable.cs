using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3913RemoveCascadeDeleteOnKeyStatDataBlockTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeyStatisticsDataBlock_ContentBlock_DataBlockId",
                table: "KeyStatisticsDataBlock");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStatisticsDataBlock_ContentBlock_DataBlockId",
                table: "KeyStatisticsDataBlock",
                column: "DataBlockId",
                principalTable: "ContentBlock",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeyStatisticsDataBlock_ContentBlock_DataBlockId",
                table: "KeyStatisticsDataBlock");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyStatisticsDataBlock_ContentBlock_DataBlockId",
                table: "KeyStatisticsDataBlock",
                column: "DataBlockId",
                principalTable: "ContentBlock",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
