using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

/// <inheritdoc />
public partial class EES4770_AddReleaseFilesPublishedColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "Published",
            table: "ReleaseFiles",
            type: "datetime2",
            nullable: true);

        migrationBuilder.Sql(@"
                UPDATE ReleaseFiles
                SET ReleaseFiles.Published = ReleaseVersions.Published
                FROM ReleaseFiles
                JOIN ReleaseVersions ON ReleaseVersions.Id = ReleaseFiles.ReleaseVersionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Published",
            table: "ReleaseFiles");
    }
}
