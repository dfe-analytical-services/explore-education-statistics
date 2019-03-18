using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddRegionCodeIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Region, '$.region_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Region, '$.region_code')");

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Region, '$.region_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Region, '$.region_code')");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_RegionCode",
                table: "GeographicData",
                column: "RegionCode");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_RegionCode",
                table: "CharacteristicDataLa",
                column: "RegionCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeographicData_RegionCode",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_RegionCode",
                table: "CharacteristicDataLa");

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Region, '$.region_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Region, '$.region_code')");

            migrationBuilder.AlterColumn<string>(
                name: "RegionCode",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Region, '$.region_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(Region, '$.region_code')");
        }
    }
}
