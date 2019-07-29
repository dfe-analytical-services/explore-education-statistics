using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class AddFilterItemFootnote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_FilterItemFootnote_FootnoteId",
                table: "FilterItemFootnote",
                column: "FootnoteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterItemFootnote");
        }
    }
}
