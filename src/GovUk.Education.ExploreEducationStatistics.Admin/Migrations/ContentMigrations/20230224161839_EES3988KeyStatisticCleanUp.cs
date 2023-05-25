using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES3988KeyStatisticCleanUp : Migration
    {
        private const string MigrationId = "20230224161839";
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentBlockIdTemp",
                table: "KeyStatistics");

            migrationBuilder.DropColumn(
                name: "DataBlock_Summary",
                table: "ContentBlock");

            migrationBuilder.SqlFromFile(ContentMigrationsPath, $"{MigrationId}_CleanUpKeyStatData.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContentBlockIdTemp",
                table: "KeyStatistics",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "DataBlock_Summary",
                table: "ContentBlock",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
