using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public partial class Ees6832RenameUpdatePublishedDateToUpdatePublishedDisplayDate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "UpdatePublishedDate",
            table: "ReleaseVersions",
            newName: "UpdatePublishedDisplayDate"
        );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "UpdatePublishedDisplayDate",
            table: "ReleaseVersions",
            newName: "UpdatePublishedDate"
        );
    }
}
