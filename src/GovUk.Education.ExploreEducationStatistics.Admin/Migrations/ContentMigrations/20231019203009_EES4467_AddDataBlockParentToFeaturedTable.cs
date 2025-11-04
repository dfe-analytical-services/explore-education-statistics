using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4467_AddDataBlockParentToFeaturedTable : Migration
{
    private const string MigrationId = "20231019203009";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "DataBlockParentId",
            table: "FeaturedTables",
            type: "uniqueidentifier",
            nullable: true
        );

        migrationBuilder.CreateIndex(
            name: "IX_FeaturedTables_DataBlockParentId",
            table: "FeaturedTables",
            column: "DataBlockParentId"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_FeaturedTables_DataBlocks_DataBlockParentId",
            table: "FeaturedTables",
            column: "DataBlockParentId",
            principalTable: "DataBlocks",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
        );

        migrationBuilder.SqlFromFile(
            ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES4467_AddDataBlockParentToFeaturedTable)}.sql"
        );

        migrationBuilder.AlterColumn<Guid>(
            name: "DataBlockParentId",
            table: "FeaturedTables",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_FeaturedTables_DataBlocks_DataBlockParentId",
            table: "FeaturedTables"
        );

        migrationBuilder.DropIndex(name: "IX_FeaturedTables_DataBlockParentId", table: "FeaturedTables");

        migrationBuilder.DropColumn(name: "DataBlockParentId", table: "FeaturedTables");
    }
}
