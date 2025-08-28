#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
public partial class EES4668_RenameReleaseParentToRelease : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename ReleaseVersions indexes which were not renamed by the Releases -> ReleaseVersions table rename
        // in migration 20240229162115_EES4668_RenameReleaseToReleaseVersion
        migrationBuilder.RenameIndex(
            name: "PK_Releases",
            table: "ReleaseVersions",
            newName: "PK_ReleaseVersions");

        migrationBuilder.RenameIndex(
            name: "IX_Releases_PublicationId",
            table: "ReleaseVersions",
            newName: "IX_ReleaseVersions_PublicationId");

        migrationBuilder.RenameIndex(
            name: "IX_Releases_CreatedById",
            table: "ReleaseVersions",
            newName: "IX_ReleaseVersions_CreatedById");

        migrationBuilder.RenameIndex(
            name: "IX_Releases_PreviousVersionId_Version",
            table: "ReleaseVersions",
            newName: "IX_ReleaseVersions_PreviousVersionId_Version");

        migrationBuilder.RenameIndex(
            name: "IX_Releases_Type",
            table: "ReleaseVersions",
            newName: "IX_ReleaseVersions_Type");

        migrationBuilder.RenameIndex(
            name: "IX_Releases_ReleaseParentId",
            table: "ReleaseVersions",
            newName: "IX_ReleaseVersions_ReleaseParentId");

        // Rename foreign key constraints which were not renamed by the Releases -> ReleaseVersions table rename
        // in migration 20240229162115_EES4668_RenameReleaseToReleaseVersion
        migrationBuilder.Sql("EXEC sp_rename 'dbo.FK_Releases_Publications_PublicationId', 'FK_ReleaseVersions_Publications_PublicationId', 'OBJECT'");
        migrationBuilder.Sql("EXEC sp_rename 'dbo.FK_Releases_ReleaseParents_ReleaseParentId', 'FK_ReleaseVersions_ReleaseParents_ReleaseParentId', 'OBJECT'");
        migrationBuilder.Sql("EXEC sp_rename 'dbo.FK_Releases_Releases_PreviousVersionId', 'FK_ReleaseVersions_Releases_PreviousVersionId', 'OBJECT'");
        migrationBuilder.Sql("EXEC sp_rename 'dbo.FK_Releases_Users_CreatedById', 'FK_ReleaseVersions_Users_CreatedById', 'OBJECT'");

        // Drop the ReleaseVersions foreign key constraint
        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseVersions_ReleaseParents_ReleaseParentId",
            table: "ReleaseVersions");

        // Rename the ReleaseParents table to Releases
        migrationBuilder.RenameTable(
            name: "ReleaseParents",
            newName: "Releases");

        // Rename indexes which are not renamed by the ReleaseParents -> Releases table rename
        migrationBuilder.RenameIndex(
            name: "PK_ReleaseParents",
            table: "Releases",
            newName: "PK_Releases");

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
