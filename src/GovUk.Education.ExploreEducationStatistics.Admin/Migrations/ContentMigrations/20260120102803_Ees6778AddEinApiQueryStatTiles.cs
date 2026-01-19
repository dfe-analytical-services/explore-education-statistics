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
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "DataSetId",
                table: "EinTiles",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(name: "DecimalPlaces", table: "EinTiles", type: "int", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EinApiQueryStatTile_Statistic",
                table: "EinTiles",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "IndicatorUnit",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "LatestPublishedVersion",
                table: "EinTiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "PublicationSlug",
                table: "EinTiles",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(name: "Query", table: "EinTiles", type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryResult",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "ReleaseSlug",
                table: "EinTiles",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "EinTiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataSetId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "DecimalPlaces", table: "EinTiles");

            migrationBuilder.DropColumn(name: "EinApiQueryStatTile_Statistic", table: "EinTiles");

            migrationBuilder.DropColumn(name: "IndicatorUnit", table: "EinTiles");

            migrationBuilder.DropColumn(name: "LatestPublishedVersion", table: "EinTiles");

            migrationBuilder.DropColumn(name: "PublicationSlug", table: "EinTiles");

            migrationBuilder.DropColumn(name: "Query", table: "EinTiles");

            migrationBuilder.DropColumn(name: "QueryResult", table: "EinTiles");

            migrationBuilder.DropColumn(name: "ReleaseSlug", table: "EinTiles");

            migrationBuilder.DropColumn(name: "Version", table: "EinTiles");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048
            );
        }
    }
}
