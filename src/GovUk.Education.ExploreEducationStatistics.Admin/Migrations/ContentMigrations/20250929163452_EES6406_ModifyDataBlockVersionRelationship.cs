using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
// ReSharper disable once InconsistentNaming
public partial class EES6406_ModifyDataBlockVersionRelationship : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Replace the existing non-unique index on DataBlockVersions.ContentBlockId with a unique index,
        // since there is a one-to-one relationship between DataBlockVersion and DataBlock.
        migrationBuilder.DropIndex(name: "IX_DataBlockVersions_ContentBlockId", table: "DataBlockVersions");

        migrationBuilder.CreateIndex(
            name: "IX_DataBlockVersions_ContentBlockId",
            table: "DataBlockVersions",
            column: "ContentBlockId",
            unique: true
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_DataBlockVersions_ContentBlockId", table: "DataBlockVersions");

        migrationBuilder.CreateIndex(
            name: "IX_DataBlockVersions_ContentBlockId",
            table: "DataBlockVersions",
            column: "ContentBlockId"
        );
    }
}
