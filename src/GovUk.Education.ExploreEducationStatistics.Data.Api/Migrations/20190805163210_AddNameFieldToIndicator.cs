using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddNameFieldToIndicator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Indicator",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Indicator_Name",
                table: "Indicator",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Indicator_Name",
                table: "Indicator");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Indicator");
        }
    }
}
