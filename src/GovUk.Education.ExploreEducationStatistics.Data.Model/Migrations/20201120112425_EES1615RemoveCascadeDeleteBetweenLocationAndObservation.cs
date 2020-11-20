using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1615RemoveCascadeDeleteBetweenLocationAndObservation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observation_Location_LocationId",
                table: "Observation");

            migrationBuilder.AddForeignKey(
                name: "FK_Observation_Location_LocationId",
                table: "Observation",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Observation_Location_LocationId",
                table: "Observation");

            migrationBuilder.AddForeignKey(
                name: "FK_Observation_Location_LocationId",
                table: "Observation",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
