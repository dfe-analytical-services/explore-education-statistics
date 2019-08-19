using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class AddFootnotes : Migration
    {
        private const string _migrationsPath = "Migrations/";
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Footnote",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Footnote", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IndicatorFootnote",
                columns: table => new
                {
                    IndicatorId = table.Column<long>(nullable: false),
                    FootnoteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicatorFootnote", x => new { x.IndicatorId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_IndicatorFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndicatorFootnote_Indicator_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "Indicator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectFootnote",
                columns: table => new
                {
                    SubjectId = table.Column<long>(nullable: false),
                    FootnoteId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectFootnote", x => new { x.SubjectId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_SubjectFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubjectFootnote_Subject_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subject",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndicatorFootnote_FootnoteId",
                table: "IndicatorFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectFootnote_FootnoteId",
                table: "SubjectFootnote",
                column: "FootnoteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndicatorFootnote");

            migrationBuilder.DropTable(
                name: "SubjectFootnote");

            migrationBuilder.DropTable(
                name: "Footnote");
        }
        
        private static void ExecuteFile(MigrationBuilder migrationBuilder, string filename)
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), filename);
            migrationBuilder.Sql(File.ReadAllText(file));
        }
    }
}
