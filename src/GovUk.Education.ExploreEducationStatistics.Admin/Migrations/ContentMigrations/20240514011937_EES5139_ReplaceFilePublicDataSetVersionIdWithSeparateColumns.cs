#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5139_ReplaceFilePublicDataSetVersionIdWithSeparateColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Files_PublicDataSetVersionId",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "PublicDataSetVersionId",
            table: "Files");

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
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Files_PublicApiDataSetId_PublicApiDataSetVersion",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "PublicApiDataSetVersion",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "PublicApiDataSetId",
            table: "Files");

        migrationBuilder.AddColumn<Guid>(
            name: "PublicDataSetVersionId",
            table: "Files");

        migrationBuilder.CreateIndex(
            name: "IX_Files_PublicDataSetVersionId",
            table: "Files",
            column: "PublicDataSetVersionId");
    }
}
