using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations.MigrationConstants;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES820_Add_ReleaseFootnote_Table : Migration
    {
        private const string MigrationId = "20200520085437";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseFootnote",
                columns: table => new
                {
                    FootnoteId = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFootnote", x => new { x.ReleaseId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReleaseFootnote_Release_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Release",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFootnote_FootnoteId",
                table: "ReleaseFootnote",
                column: "FootnoteId");
            
            migrationBuilder.SqlFromFile(MigrationsPath, $"{MigrationId}_ReleaseFootnote_Up.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseFootnote");
        }
    }
}
