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
            
            migrationBuilder.RenameColumn("ReleaseDate",
                "Release",
                "Published");

            migrationBuilder.AlterColumn<DateTime>("Published", "Release", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>("Published", "Release", nullable: false);
            
            migrationBuilder.RenameColumn("Published",
                "Release",
                "ReleaseDate");

            // Revert to the version in the previous migration 20200217131418_Routine_DropAndCreateRelease.sql
            migrationBuilder.Sql("DROP PROCEDURE dbo.DropAndCreateRelease");
            migrationBuilder.SqlFromFile(MigrationsPath,
                $"{PreviousVersionMigrationId}_Routine_DropAndCreateRelease.sql");
        }
    }
}