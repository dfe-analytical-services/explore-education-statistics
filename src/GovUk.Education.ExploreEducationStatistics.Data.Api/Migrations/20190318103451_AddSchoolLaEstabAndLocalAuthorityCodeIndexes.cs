using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddSchoolLaEstabAndLocalAuthorityCodeIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SchoolLaEstab",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(School, '$.laestab')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(School, '$.laestab')");

            migrationBuilder.AlterColumn<string>(
                name: "LocalAuthorityCode",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')");

            migrationBuilder.AlterColumn<string>(
                name: "LocalAuthorityCode",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_LocalAuthorityCode",
                table: "GeographicData",
                column: "LocalAuthorityCode");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_SchoolLaEstab",
                table: "GeographicData",
                column: "SchoolLaEstab");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_LocalAuthorityCode",
                table: "CharacteristicDataLa",
                column: "LocalAuthorityCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeographicData_LocalAuthorityCode",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_GeographicData_SchoolLaEstab",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_LocalAuthorityCode",
                table: "CharacteristicDataLa");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolLaEstab",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(School, '$.laestab')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(School, '$.laestab')");

            migrationBuilder.AlterColumn<string>(
                name: "LocalAuthorityCode",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')");

            migrationBuilder.AlterColumn<string>(
                name: "LocalAuthorityCode",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')",
                oldClrType: typeof(string),
                oldNullable: true,
                oldComputedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')");
        }
    }
}
