using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4467_AddDataBlockParentToKeyStatisticDataBlock : Migration
    {
        private const string MigrationId = "20231028190916";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DataBlockParentId",
                table: "KeyStatisticsDataBlock",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.SqlFromFile(
                ContentMigrationsPath,
                $"{MigrationId}_{nameof(EES4467_AddDataBlockParentToKeyStatisticDataBlock)}.sql");

            migrationBuilder.AlterColumn<Guid>(
                name: "DataBlockParentId",
                table: "KeyStatisticsDataBlock",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataBlockParentId",
                table: "KeyStatisticsDataBlock");
        }
    }
}
