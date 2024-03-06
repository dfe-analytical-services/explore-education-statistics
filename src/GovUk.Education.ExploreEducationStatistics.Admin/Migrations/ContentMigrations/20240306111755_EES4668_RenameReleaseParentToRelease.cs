using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4668_RenameReleaseParentToRelease : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Drop the ReleaseVersions foreign key constraint
        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseVersions_ReleaseParents_ReleaseParentId",
            table: "ReleaseVersions");

        // Rename the ReleaseParents table to Releases
        migrationBuilder.RenameTable(
            name: "ReleaseParents",
            newName: "Releases");

        // Rename the ReleaseVersions.ReleaseParentId column
        migrationBuilder.RenameColumn(
            name: "ReleaseParentId",
            table: "ReleaseVersions",
            newName: "ReleaseId");

        // Rename the index on the ReleaseVersions.ReleaseParentId foreign key column
        migrationBuilder.RenameIndex(
            name: "IX_ReleaseVersions_ReleaseParentId",
            table: "ReleaseVersions",
            newName: "IX_ReleaseVersions_ReleaseId");

        // Recreate the ReleaseVersions foreign key constraint
        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseVersions_Releases_ReleaseId",
            table: "ReleaseVersions",
            column: "ReleaseId",
            principalTable: "Releases",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
