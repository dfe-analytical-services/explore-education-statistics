using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddCharacteristicNameComputedValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CharacteristicName",
                table: "CharacteristicDataNational",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')");

            migrationBuilder.AddColumn<string>(
                name: "CharacteristicName",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_CharacteristicName",
                table: "CharacteristicDataNational",
                column: "CharacteristicName");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_CharacteristicName",
                table: "CharacteristicDataLa",
                column: "CharacteristicName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataNational_CharacteristicName",
                table: "CharacteristicDataNational");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_CharacteristicName",
                table: "CharacteristicDataLa");

            migrationBuilder.DropColumn(
                name: "CharacteristicName",
                table: "CharacteristicDataNational");

            migrationBuilder.DropColumn(
                name: "CharacteristicName",
                table: "CharacteristicDataLa");
        }
    }
}
