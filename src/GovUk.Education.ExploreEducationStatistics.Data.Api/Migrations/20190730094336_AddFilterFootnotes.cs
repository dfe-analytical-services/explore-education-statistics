using System.IO;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddFilterFootnotes : Migration
    {
        private const string _migrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilterFootnote",
                columns: table => new
                {
                    FilterId = table.Column<long>(nullable: false),
                    FootnoteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterFootnote", x => new { x.FilterId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_FilterFootnote_Filter_FilterId",
                        column: x => x.FilterId,
                        principalTable: "Filter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilterGroupFootnote",
                columns: table => new
                {
                    FilterGroupId = table.Column<long>(nullable: false),
                    FootnoteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterGroupFootnote", x => new { x.FilterGroupId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_FilterGroupFootnote_FilterGroup_FilterGroupId",
                        column: x => x.FilterGroupId,
                        principalTable: "FilterGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterGroupFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilterItemFootnote",
                columns: table => new
                {
                    FilterItemId = table.Column<long>(nullable: false),
                    FootnoteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterItemFootnote", x => new { x.FilterItemId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_FilterItemFootnote_FilterItem_FilterItemId",
                        column: x => x.FilterItemId,
                        principalTable: "FilterItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilterItemFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilterFootnote_FootnoteId",
                table: "FilterFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterGroupFootnote_FootnoteId",
                table: "FilterGroupFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterItemFootnote_FootnoteId",
                table: "FilterItemFootnote",
                column: "FootnoteId");
            
            ExecuteFile(migrationBuilder, _migrationsPath + "20190730094336_FilteredFootnotes.sql");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE dbo.FilteredFootnotes");

            migrationBuilder.DropTable(
                name: "FilterFootnote");

            migrationBuilder.DropTable(
                name: "FilterGroupFootnote");

            migrationBuilder.DropTable(
                name: "FilterItemFootnote");
        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
