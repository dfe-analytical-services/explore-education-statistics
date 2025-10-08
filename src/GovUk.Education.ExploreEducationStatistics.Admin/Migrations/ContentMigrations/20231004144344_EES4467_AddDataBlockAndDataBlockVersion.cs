using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Admin.Migrations.MigrationConstants;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations;

// ReSharper disable once InconsistentNaming
public partial class EES4467_AddDataBlockAndDataBlockVersion : Migration
{
    private const string MigrationId = "20231004144344";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataBlocks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                LatestDraftVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LatestPublishedVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataBlocks", x => x.Id);
            }
        );

        migrationBuilder.CreateTable(
            name: "DataBlockVersions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DataBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ContentBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                Published = table.Column<DateTime>(type: "datetime2", nullable: true),
                Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DataBlockVersions", x => x.Id);
                table.ForeignKey(
                    name: "FK_DataBlockVersions_ContentBlock_ContentBlockId",
                    column: x => x.ContentBlockId,
                    principalTable: "ContentBlock",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction
                );
                table.ForeignKey(
                    name: "FK_DataBlockVersions_DataBlocks_DataBlockId",
                    column: x => x.DataBlockId,
                    principalTable: "DataBlocks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
                table.ForeignKey(
                    name: "FK_DataBlockVersions_Releases_ReleaseId",
                    column: x => x.ReleaseId,
                    principalTable: "Releases",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade
                );
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataBlocks_LatestPublishedVersionId",
            table: "DataBlocks",
            column: "LatestPublishedVersionId",
            unique: true,
            filter: "[LatestPublishedVersionId] IS NOT NULL"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataBlocks_LatestDraftVersionId",
            table: "DataBlocks",
            column: "LatestDraftVersionId",
            unique: true,
            filter: "[LatestDraftVersionId] IS NOT NULL"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataBlockVersions_ContentBlockId",
            table: "DataBlockVersions",
            column: "ContentBlockId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataBlockVersions_DataBlockId",
            table: "DataBlockVersions",
            column: "DataBlockId"
        );

        migrationBuilder.CreateIndex(
            name: "IX_DataBlockVersions_ReleaseId",
            table: "DataBlockVersions",
            column: "ReleaseId"
        );

        migrationBuilder.SqlFromFile(
            ContentMigrationsPath,
            $"{MigrationId}_{nameof(EES4467_AddDataBlockAndDataBlockVersion)}.sql"
        );

        migrationBuilder.AddForeignKey(
            name: "FK_DataBlocks_DataBlockVersions_LatestPublishedVersionId",
            table: "DataBlocks",
            column: "LatestPublishedVersionId",
            principalTable: "DataBlockVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction
        );

        migrationBuilder.AddForeignKey(
            name: "FK_DataBlocks_DataBlockVersions_LatestDraftVersionId",
            table: "DataBlocks",
            column: "LatestDraftVersionId",
            principalTable: "DataBlockVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction
        );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DataBlocks_DataBlockVersions_LatestPublishedVersionId",
            table: "DataBlocks"
        );

        migrationBuilder.DropForeignKey(
            name: "FK_DataBlocks_DataBlockVersions_LatestDraftVersionId",
            table: "DataBlocks"
        );

        migrationBuilder.DropTable(name: "DataBlockVersions");

        migrationBuilder.DropTable(name: "DataBlocks");
    }
}
