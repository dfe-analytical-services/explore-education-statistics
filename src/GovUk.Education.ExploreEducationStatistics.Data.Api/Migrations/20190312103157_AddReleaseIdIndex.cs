using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddReleaseIdIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_ReleaseId",
                table: "GeographicData",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_ReleaseId",
                table: "CharacteristicDataNational",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_ReleaseId",
                table: "CharacteristicDataLa",
                column: "ReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeographicData_ReleaseId",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataNational_ReleaseId",
                table: "CharacteristicDataNational");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_ReleaseId",
                table: "CharacteristicDataLa");
        }
    }
}
