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
            migrationBuilder.AddColumn<Guid>(
                name: "DataSetId",
                table: "EinTiles",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(name: "DecimalPlaces", table: "EinTiles", type: "int", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EinApiQueryStatTile_Title",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<int>(
                name: "IndicatorUnit",
                table: "EinTiles",
                type: "int", // @MarkFix want this to be a string
                nullable: true
            );

            migrationBuilder.AddColumn<bool>(name: "IsLatestVersion", table: "EinTiles", type: "bit", nullable: true);

            migrationBuilder.AddColumn<string>(name: "Query", table: "EinTiles", type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryResult",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DataSetId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "DecimalPlaces", table: "EinTiles");

            migrationBuilder.DropColumn(name: "EinApiQueryStatTile_Title", table: "EinTiles");

            migrationBuilder.DropColumn(name: "IndicatorUnit", table: "EinTiles");

            migrationBuilder.DropColumn(name: "IsLatestVersion", table: "EinTiles");

            migrationBuilder.DropColumn(name: "Query", table: "EinTiles");

            migrationBuilder.DropColumn(name: "QueryResult", table: "EinTiles");

            migrationBuilder.DropColumn(name: "Version", table: "EinTiles");
        }
    }
}
