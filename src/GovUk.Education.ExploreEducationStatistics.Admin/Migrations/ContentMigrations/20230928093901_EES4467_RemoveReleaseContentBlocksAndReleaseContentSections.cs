using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4467_RemoveReleaseContentBlocksAndReleaseContentSections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseContentBlocks");

            migrationBuilder.DropTable(
                name: "ReleaseContentSections");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseContentBlocks",
                columns: table => new
                {
                    ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContentBlocks", x => new { x.ReleaseId, x.ContentBlockId });
                    table.ForeignKey(
                        name: "FK_ReleaseContentBlocks_ContentBlock_ContentBlockId",
                        column: x => x.ContentBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseContentBlocks_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseContentSections",
                columns: table => new
                {
                    ReleaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentSectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseContentSections", x => new { x.ReleaseId, x.ContentSectionId });
                    table.ForeignKey(
                        name: "FK_ReleaseContentSections_ContentSections_ContentSectionId",
                        column: x => x.ContentSectionId,
                        principalTable: "ContentSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseContentSections_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseContentBlocks_ContentBlockId",
                table: "ReleaseContentBlocks",
                column: "ContentBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseContentSections_ContentSectionId",
                table: "ReleaseContentSections",
                column: "ContentSectionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentSections_Releases_ReleaseId",
                table: "ContentSections",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id");
        }
    }
}
