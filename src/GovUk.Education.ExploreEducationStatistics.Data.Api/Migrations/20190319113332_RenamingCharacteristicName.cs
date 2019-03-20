using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class RenamingCharacteristicName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CharacteristicName",
                table: "CharacteristicDataNational",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_breakdown')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')");

            migrationBuilder.AlterColumn<string>(
                name: "CharacteristicName",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_breakdown')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CharacteristicName",
                table: "CharacteristicDataNational",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_breakdown')");

            migrationBuilder.AlterColumn<string>(
                name: "CharacteristicName",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_1')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Characteristic, '$.characteristic_breakdown')");
        }
    }
}
