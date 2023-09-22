using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4467_AddDataBlocks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBlocks", x => x.Id);
                });

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
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataBlockVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataBlockVersions_ContentBlock_DataBlockId",
                        column: x => x.DataBlockId,
                        principalTable: "ContentBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataBlockVersions_DataBlocks_DataBlockId",
                        column: x => x.DataBlockId,
                        principalTable: "DataBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataBlockVersions_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataBlockVersions_DataBlockId",
                table: "DataBlockVersions",
                column: "DataBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_DataBlockVersions_DataBlockId",
                table: "DataBlockVersions",
                column: "DataBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_DataBlockVersions_ReleaseId",
                table: "DataBlockVersions",
                column: "ReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataBlockVersions");

            migrationBuilder.DropTable(
                name: "DataBlocks");
        }
    }
}
