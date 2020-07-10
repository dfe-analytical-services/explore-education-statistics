using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1125_Add_DataLastPublished : Migration
    {
        private const string MigrationId = "20200707114940";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataLastPublished",
                table: "Releases",
                nullable: true);
            
            migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_EES-1125_UpdateDataLastPublished.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataLastPublished",
                table: "Releases");
        }
    }
}
