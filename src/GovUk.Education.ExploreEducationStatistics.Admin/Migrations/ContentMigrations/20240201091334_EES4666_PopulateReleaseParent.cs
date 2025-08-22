#nullable disable

using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4666_PopulateReleaseParent : Migration
{
    private const string MigrationId = "20240201091334";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Populate new ReleaseParentId's for all existing Releases
        migrationBuilder.SqlFromFile(MigrationConstants.ContentMigrationsPath,
            $"{MigrationId}_PopulateReleaseParent.sql");

        migrationBuilder.DropForeignKey(
            name: "FK_Releases_ReleaseParents_ReleaseParentId",
            table: "Releases");

        // Make ReleaseParentId non-nullable
        migrationBuilder.AlterColumn<Guid>(
            name: "ReleaseParentId",
            table: "Releases",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Releases_ReleaseParents_ReleaseParentId",
            table: "Releases",
            column: "ReleaseParentId",
            principalTable: "ReleaseParents",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Releases_ReleaseParents_ReleaseParentId",
            table: "Releases");

        migrationBuilder.AlterColumn<Guid>(
            name: "ReleaseParentId",
            table: "Releases",
            type: "uniqueidentifier",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier");

        migrationBuilder.AddForeignKey(
            name: "FK_Releases_ReleaseParents_ReleaseParentId",
            table: "Releases",
            column: "ReleaseParentId",
            principalTable: "ReleaseParents",
            principalColumn: "Id");

        migrationBuilder.Sql("UPDATE Releases SET ReleaseParentId = NULL");
    }
}
