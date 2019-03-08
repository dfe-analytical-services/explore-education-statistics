using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddMoreIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocalAuthorityCode",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')");

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Region, '$.region_code')");

            migrationBuilder.AddColumn<string>(
                name: "SchoolLaEstab",
                table: "GeographicData",
                nullable: true,
                computedColumnSql: "JSON_VALUE(School, '$.laestab')");

            migrationBuilder.AddColumn<string>(
                name: "LocalAuthorityCode",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(LocalAuthority, '$.new_la_code')");

            migrationBuilder.AddColumn<string>(
                name: "RegionCode",
                table: "CharacteristicDataLa",
                nullable: true,
                computedColumnSql: "JSON_VALUE(Region, '$.region_code')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalAuthorityCode",
                table: "GeographicData");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "GeographicData");

            migrationBuilder.DropColumn(
                name: "SchoolLaEstab",
                table: "GeographicData");

            migrationBuilder.DropColumn(
                name: "LocalAuthorityCode",
                table: "CharacteristicDataLa");

            migrationBuilder.DropColumn(
                name: "RegionCode",
                table: "CharacteristicDataLa");
        }
    }
}
