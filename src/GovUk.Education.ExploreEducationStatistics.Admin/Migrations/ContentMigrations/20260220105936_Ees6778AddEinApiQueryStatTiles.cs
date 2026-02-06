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
                nullable: true,
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

            migrationBuilder.AddColumn<Guid>(
                name: "DataSetVersionId",
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

            migrationBuilder.AddColumn<Guid>(
                name: "LatestDataSetVersionId",
                table: "EinTiles",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(name: "Query", table: "EinTiles", type: "nvarchar(max)", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryResult",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true
            );

            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseId",
                table: "EinTiles",
                type: "uniqueidentifier",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "EinTiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true
            );

            migrationBuilder.CreateIndex(name: "IX_EinTiles_ReleaseId", table: "EinTiles", column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_EinTiles_Releases_ReleaseId",
                table: "EinTiles",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id"
            );

            migrationBuilder.Sql("GRANT SELECT ON dbo.EinTiles TO [publisher]");
            migrationBuilder.Sql("GRANT UPDATE ON dbo.EinTiles TO [publisher]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EinTiles_Releases_ReleaseId", table: "EinTiles");

            migrationBuilder.DropIndex(name: "IX_EinTiles_ReleaseId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "DataSetId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "DataSetVersionId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "DecimalPlaces", table: "EinTiles");

            migrationBuilder.DropColumn(name: "EinApiQueryStatTile_Statistic", table: "EinTiles");

            migrationBuilder.DropColumn(name: "IndicatorUnit", table: "EinTiles");

            migrationBuilder.DropColumn(name: "LatestDataSetVersionId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "Query", table: "EinTiles");

            migrationBuilder.DropColumn(name: "QueryResult", table: "EinTiles");

            migrationBuilder.DropColumn(name: "ReleaseId", table: "EinTiles");

            migrationBuilder.DropColumn(name: "Version", table: "EinTiles");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "EinTiles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true
            );

            migrationBuilder.Sql("REVOKE SELECT ON dbo.EinTiles TO [publisher]");
            migrationBuilder.Sql("REVOKE UPDATE ON dbo.EinTiles TO [publisher]");
        }
    }
}
