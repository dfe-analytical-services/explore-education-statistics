using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES4993_AddPublicApiDataSetIdVersionToReleaseFile : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PublicApiDataSetId",
            table: "ReleaseFiles",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PublicApiDataSetVersion",
            table: "ReleaseFiles",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ReleaseFiles_ReleaseVersionId_FileId",
            table: "ReleaseFiles",
            columns: new[] { "ReleaseVersionId", "FileId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ReleaseFiles_ReleaseVersionId_PublicApiDataSetId_PublicApiDataSetVersion",
            table: "ReleaseFiles",
            columns: new[] { "ReleaseVersionId", "PublicApiDataSetId", "PublicApiDataSetVersion" },
            unique: true,
            filter: "[PublicApiDataSetId] IS NOT NULL AND [PublicApiDataSetVersion] IS NOT NULL");

        // This can incorrectly set these columns if an amendment is created and any
        // of its ReleaseFiles are promoted to a draft API data set. This needs to
        // be rectified by an additional endpoint migration.
        migrationBuilder.Sql(
            """
            UPDATE ReleaseFiles
            SET ReleaseFiles.PublicApiDataSetId = Files.PublicApiDataSetId,
                ReleaseFiles.PublicApiDataSetVersion = Files.PublicApiDataSetVersion
            FROM dbo.ReleaseFiles
            INNER JOIN dbo.Files ON Files.Id = ReleaseFiles.FileId
            """);

        migrationBuilder.DropIndex(
            name: "IX_ReleaseFiles_ReleaseVersionId",
            table: "ReleaseFiles");

        migrationBuilder.DropIndex(
            name: "IX_Files_PublicApiDataSetId_PublicApiDataSetVersion",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "PublicApiDataSetId",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "PublicApiDataSetVersion",
            table: "Files");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PublicApiDataSetId",
            table: "Files",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "PublicApiDataSetVersion",
            table: "Files",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Files_PublicApiDataSetId_PublicApiDataSetVersion",
            table: "Files",
            columns: new[] { "PublicApiDataSetId", "PublicApiDataSetVersion" },
            unique: true,
            filter: "[PublicApiDataSetId] IS NOT NULL AND [PublicApiDataSetVersion] IS NOT NULL");

        migrationBuilder.Sql(
            """
            UPDATE Files
            SET Files.PublicApiDataSetId = ReleaseFiles.PublicApiDataSetId,
                Files.PublicApiDataSetVersion = ReleaseFiles.PublicApiDataSetVersion
            FROM dbo.ReleaseFiles
            INNER JOIN dbo.Files ON Files.Id = ReleaseFiles.FileId
            WHERE ReleaseFiles.PublicApiDataSetId IS NOT NULL
                AND ReleaseFiles.PublicApiDataSetVersion IS NOT NULL
            """);

        migrationBuilder.DropIndex(
            name: "IX_ReleaseFiles_ReleaseVersionId_FileId",
            table: "ReleaseFiles");

        migrationBuilder.DropIndex(
            name: "IX_ReleaseFiles_ReleaseVersionId_PublicApiDataSetId_PublicApiDataSetVersion",
            table: "ReleaseFiles");

        migrationBuilder.DropColumn(
            name: "PublicApiDataSetId",
            table: "ReleaseFiles");

        migrationBuilder.DropColumn(
            name: "PublicApiDataSetVersion",
            table: "ReleaseFiles");

        migrationBuilder.CreateIndex(
            name: "IX_ReleaseFiles_ReleaseVersionId",
            table: "ReleaseFiles",
            column: "ReleaseVersionId");
    }
}
