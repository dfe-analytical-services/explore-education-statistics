#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
// ReSharper disable once InconsistentNaming
public partial class EES5056_AddPublicDataSetVersionIdToFilesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "PublicDataSetVersionId",
            table: "Files",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Files_PublicDataSetVersionId",
            table: "Files",
            column: "PublicDataSetVersionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Files_PublicDataSetVersionId",
            table: "Files");

        migrationBuilder.DropColumn(
            name: "PublicDataSetVersionId",
            table: "Files");
    }
}
