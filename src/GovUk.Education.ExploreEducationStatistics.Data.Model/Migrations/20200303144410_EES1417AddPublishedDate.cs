using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1417AddPublishedDate : Migration
    {
        private const string MigrationId = "20200303144410";
        private const string PreviousVersionMigrationId = "20200217131418";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_Routine_DropAndCreateRelease.sql");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Release");

            migrationBuilder.AddColumn<DateTime>(
                name: "Published",
                table: "Release",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Published",
                table: "Release");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Release",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            // Revert to the version in the previous migration 20200217131418_Routine_DropAndCreateRelease.sql
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousVersionMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}