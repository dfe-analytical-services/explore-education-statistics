using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2776_AddGeographicLevelToLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeographicLevel",
                table: "Location",
                maxLength: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_GeographicLevel",
                table: "Location",
                column: "GeographicLevel");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Location_GeographicLevel",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "GeographicLevel",
                table: "Location");
        }
    }
}
