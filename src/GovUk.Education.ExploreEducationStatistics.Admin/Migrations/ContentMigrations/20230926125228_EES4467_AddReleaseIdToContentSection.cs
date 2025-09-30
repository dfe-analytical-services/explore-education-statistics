using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4467_AddReleaseIdToContentSection : Migration
{
    private const string MigrationId = "20230926125228";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ReleaseId",
            table: "ContentSections",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ContentSections_ReleaseId",
            table: "ContentSections",
            column: "ReleaseId");

        migrationBuilder.SqlFromFile(
            ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES4467_AddReleaseIdToContentSection)}.sql");

        // Initially create this foreign key with "Restrict" delete cascade behaviour. This is so as to not
        // cause potential cyclical delete cascades with the existing cascade delete behaviour from
        // Releases -> ReleaseContentSections -> ContentSections.
        migrationBuilder.AddForeignKey(
            name: "FK_ContentSections_Releases_ReleaseId",
            table: "ContentSections",
            column: "ReleaseId",
            principalTable: "Releases",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AlterColumn<Guid>(
            name: "ReleaseId",
            table: "ContentSections",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "uniqueidentifier",
            oldNullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ContentSections_Releases_ReleaseId",
            table: "ContentSections");

        migrationBuilder.DropIndex(
            name: "IX_ContentSections_ReleaseId",
            table: "ContentSections");

        migrationBuilder.DropColumn(
            name: "ReleaseId",
            table: "ContentSections");
    }
}
