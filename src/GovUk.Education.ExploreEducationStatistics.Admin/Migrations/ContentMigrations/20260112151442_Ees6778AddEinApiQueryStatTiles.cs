using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    /// <inheritdoc />
    public partial class Ees6778AddEinApiQueryStatTiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DataSetId",
                table: "EinTiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecimalPlaces",
                table: "EinTiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IndicatorUnit",
                table: "EinTiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLatestVersion",
                table: "EinTiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaResult",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Query",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryResult",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSetId",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "DecimalPlaces",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "IndicatorUnit",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "IsLatestVersion",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "MetaResult",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "Query",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "QueryResult",
                table: "EinTiles");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "EinTiles");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
