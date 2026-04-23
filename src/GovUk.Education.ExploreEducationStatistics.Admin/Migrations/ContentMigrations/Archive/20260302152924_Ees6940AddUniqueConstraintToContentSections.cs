using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class Ees6940AddUniqueConstraintToContentSections : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_ContentSections_ReleaseVersionId", table: "ContentSections");

        migrationBuilder.CreateIndex(
            name: "IX_ContentSections_ReleaseVersionId_Type",
            table: "ContentSections",
            columns: ["ReleaseVersionId", "Type"],
            unique: true,
            filter: "[Type] <> 'Generic'"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_ContentSections_ReleaseVersionId_Type", table: "ContentSections");

        migrationBuilder.CreateIndex(
            name: "IX_ContentSections_ReleaseVersionId",
            table: "ContentSections",
            column: "ReleaseVersionId"
        );
    }
}
