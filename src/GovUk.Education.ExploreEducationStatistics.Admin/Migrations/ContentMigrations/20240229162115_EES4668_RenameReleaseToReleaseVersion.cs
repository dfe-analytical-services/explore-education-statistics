#nullable disable

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES4668_RenameReleaseToReleaseVersion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // As well as renaming the Release table, 12 foreign keys referencing ReleaseId need to be renamed.
        // They are:
        // - ContentBlock.ReleaseId
        // - ContentSections.ReleaseId
        // - DataBlockVersions.ReleaseId
        // - FeaturedTables.ReleaseId
        // - KeyStatistics.ReleaseId
        // - MethodologyVersions.ScheduledWithReleaseId
        // - Publications.LatestPublishedReleaseId
        // - ReleaseFiles.ReleaseId
        // - ReleaseStatus.ReleaseId
        // - Update.ReleaseId
        // - UserReleaseInvites.ReleaseId
        // - UserReleaseRoles.ReleaseId

        // Drop the 12 foreign key constraints
        migrationBuilder.DropForeignKey(
            name: "FK_ContentBlock_Releases_ReleaseId",
            table: "ContentBlock");

        migrationBuilder.DropForeignKey(
            name: "FK_ContentSections_Releases_ReleaseId",
            table: "ContentSections");

        migrationBuilder.DropForeignKey(
            name: "FK_DataBlockVersions_Releases_ReleaseId",
            table: "DataBlockVersions");

        migrationBuilder.DropForeignKey(
            name: "FK_FeaturedTables_Releases_ReleaseId",
            table: "FeaturedTables");

        migrationBuilder.DropForeignKey(
            name: "FK_KeyStatistics_Releases_ReleaseId",
            table: "KeyStatistics");

        // Note, the name of this foreign key was incorrect
        migrationBuilder.DropForeignKey(
            name: "FK_Methodologies_Releases_ScheduledWithReleaseId",
            table: "MethodologyVersions");

        migrationBuilder.DropForeignKey(
            name: "FK_Publications_Releases_LatestPublishedReleaseId",
            table: "Publications");

        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseFiles_Releases_ReleaseId",
            table: "ReleaseFiles");

        migrationBuilder.DropForeignKey(
            name: "FK_ReleaseStatus_Releases_ReleaseId",
            table: "ReleaseStatus");

        migrationBuilder.DropForeignKey(
            name: "FK_Update_Releases_ReleaseId",
            table: "Update");

        migrationBuilder.DropForeignKey(
            name: "FK_UserReleaseInvites_Releases_ReleaseId",
            table: "UserReleaseInvites");

        migrationBuilder.DropForeignKey(
            name: "FK_UserReleaseRoles_Releases_ReleaseId",
            table: "UserReleaseRoles");

        // Drop the unique index on Publications.LatestPublishedReleaseId
        // so the column can be renamed
        migrationBuilder.DropIndex(
            name: "IX_Publications_LatestPublishedReleaseId",
            table: "Publications");

        // Rename the Releases table to ReleaseVersions
        migrationBuilder.RenameTable(
            name: "Releases",
            newName: "ReleaseVersions");

        // Rename the ReleaseId column to ReleaseVersionId in the 12 foreign key columns
        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "ContentBlock",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "ContentSections",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "DataBlockVersions",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "FeaturedTables",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "KeyStatistics",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ScheduledWithReleaseId",
            table: "MethodologyVersions",
            newName: "ScheduledWithReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "LatestPublishedReleaseId",
            table: "Publications",
            newName: "LatestPublishedReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "ReleaseFiles",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "ReleaseStatus",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "Update",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "UserReleaseInvites",
            newName: "ReleaseVersionId");

        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            newName: "ReleaseVersionId",
            table: "UserReleaseRoles");

        // Recreate the unique index on Publications.LatestPublishedReleaseVersionId
        migrationBuilder.CreateIndex(
            name: "IX_Publications_LatestPublishedReleaseVersionId",
            table: "Publications",
            column: "LatestPublishedReleaseVersionId",
            unique: true,
            filter: "[LatestPublishedReleaseVersionId] IS NOT NULL");

        // Rename the remaining 11 indexes on the foreign key columns
        migrationBuilder.RenameIndex(
            name: "IX_ContentBlock_ReleaseId",
            table: "ContentBlock",
            newName: "IX_ContentBlock_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_ContentSections_ReleaseId",
            table: "ContentSections",
            newName: "IX_ContentSections_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_DataBlockVersions_ReleaseId",
            table: "DataBlockVersions",
            newName: "IX_DataBlockVersions_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_FeaturedTables_ReleaseId",
            table: "FeaturedTables",
            newName: "IX_FeaturedTables_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_KeyStatistics_ReleaseId",
            table: "KeyStatistics",
            newName: "IX_KeyStatistics_ReleaseVersionId");

        // Note, the name of this index was incorrect
        migrationBuilder.RenameIndex(
            name: "IX_Methodologies_ScheduledWithReleaseId",
            table: "MethodologyVersions",
            newName: "IX_MethodologyVersions_ScheduledWithReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_ReleaseFiles_ReleaseId",
            table: "ReleaseFiles",
            newName: "IX_ReleaseFiles_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_ReleaseStatus_ReleaseId",
            table: "ReleaseStatus",
            newName: "IX_ReleaseStatus_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_Update_ReleaseId",
            table: "Update",
            newName: "IX_Update_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_UserReleaseInvites_ReleaseId",
            table: "UserReleaseInvites",
            newName: "IX_UserReleaseInvites_ReleaseVersionId");

        migrationBuilder.RenameIndex(
            name: "IX_UserReleaseRoles_ReleaseId",
            table: "UserReleaseRoles",
            newName: "IX_UserReleaseRoles_ReleaseVersionId");

        // Rename the ReleaseId column on the Permalinks table to ReleaseVersionId
        // (no foreign key constraint to drop here)
        migrationBuilder.RenameColumn(
            name: "ReleaseId",
            table: "Permalinks",
            newName: "ReleaseVersionId");

        // Rename the ReleaseId index on the Permalinks table
        migrationBuilder.RenameIndex(
            name: "IX_Permalinks_ReleaseId",
            table: "Permalinks",
            newName: "IX_Permalinks_ReleaseVersionId");

        // Recreate the 12 foreign key constraints
        migrationBuilder.AddForeignKey(
            name: "FK_ContentBlock_ReleaseVersions_ReleaseVersionId",
            table: "ContentBlock",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ContentSections_ReleaseVersions_ReleaseVersionId",
            table: "ContentSections",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_DataBlockVersions_ReleaseVersions_ReleaseVersionId",
            table: "DataBlockVersions",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        // Note, the 'On delete' action here is inconsistent with EF model configuration but matches current db state
        // set by migration 20230928134022_EES4467_AddCascadeDeletesToContentBlockAndContentSectionReleaseIdColumn.
        migrationBuilder.AddForeignKey(
            name: "FK_FeaturedTables_ReleaseVersions_ReleaseVersionId",
            table: "FeaturedTables",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_KeyStatistics_ReleaseVersions_ReleaseVersionId",
            table: "KeyStatistics",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_MethodologyVersions_ReleaseVersions_ScheduledWithReleaseVersionId",
            table: "MethodologyVersions",
            column: "ScheduledWithReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Publications_ReleaseVersions_LatestPublishedReleaseVersionId",
            table: "Publications",
            column: "LatestPublishedReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseFiles_ReleaseVersions_ReleaseVersionId",
            table: "ReleaseFiles",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_ReleaseStatus_ReleaseVersions_ReleaseVersionId",
            table: "ReleaseStatus",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Update_ReleaseVersions_ReleaseVersionId",
            table: "Update",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_UserReleaseInvites_ReleaseVersions_ReleaseVersionId",
            table: "UserReleaseInvites",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_UserReleaseRoles_ReleaseVersions_ReleaseVersionId",
            table: "UserReleaseRoles",
            column: "ReleaseVersionId",
            principalTable: "ReleaseVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
