using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "GeographicData",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "GeographicData",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "CharacteristicDataNational",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "CharacteristicDataNational",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "CharacteristicDataLa",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "CharacteristicDataLa",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_Level",
                table: "GeographicData",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_PublicationId",
                table: "GeographicData",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_SchoolType",
                table: "GeographicData",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_GeographicData_Year",
                table: "GeographicData",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_Level",
                table: "CharacteristicDataNational",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_PublicationId",
                table: "CharacteristicDataNational",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_SchoolType",
                table: "CharacteristicDataNational",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataNational_Year",
                table: "CharacteristicDataNational",
                column: "Year");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_Level",
                table: "CharacteristicDataLa",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_PublicationId",
                table: "CharacteristicDataLa",
                column: "PublicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_SchoolType",
                table: "CharacteristicDataLa",
                column: "SchoolType");

            migrationBuilder.CreateIndex(
                name: "IX_CharacteristicDataLa_Year",
                table: "CharacteristicDataLa",
                column: "Year");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GeographicData_Level",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_GeographicData_PublicationId",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_GeographicData_SchoolType",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_GeographicData_Year",
                table: "GeographicData");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataNational_Level",
                table: "CharacteristicDataNational");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataNational_PublicationId",
                table: "CharacteristicDataNational");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataNational_SchoolType",
                table: "CharacteristicDataNational");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataNational_Year",
                table: "CharacteristicDataNational");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_Level",
                table: "CharacteristicDataLa");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_PublicationId",
                table: "CharacteristicDataLa");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_SchoolType",
                table: "CharacteristicDataLa");

            migrationBuilder.DropIndex(
                name: "IX_CharacteristicDataLa_Year",
                table: "CharacteristicDataLa");

            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "GeographicData",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "GeographicData",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "CharacteristicDataNational",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "CharacteristicDataNational",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "SchoolType",
                table: "CharacteristicDataLa",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Level",
                table: "CharacteristicDataLa",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
