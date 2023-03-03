using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES3763_DropUnusedColumnsFromStatisticsRelease : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Release_PreviousVersionId",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "PreviousVersionId",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Published",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "TimeIdentifier",
                table: "Release");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Release");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreviousVersionId",
                table: "Release",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Published",
                table: "Release",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Release",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeIdentifier",
                table: "Release",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Release",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Release_PreviousVersionId",
                table: "Release",
                column: "PreviousVersionId");
        }
    }
}
