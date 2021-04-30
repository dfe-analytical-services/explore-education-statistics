using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES2114AddLocationAndTimePeriodFootnoteTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationFootnote",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationFootnote", x => new { x.LocationId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_LocationFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationFootnote_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimePeriodFootnote",
                columns: table => new
                {
                    Year = table.Column<int>(nullable: false),
                    TimeIdentifier = table.Column<int>(nullable: false),
                    FootnoteId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimePeriodFootnote", x => new { x.Year, x.TimeIdentifier, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_TimePeriodFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationFootnote_FootnoteId",
                table: "LocationFootnote",
                column: "FootnoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TimePeriodFootnote_FootnoteId",
                table: "TimePeriodFootnote",
                column: "FootnoteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationFootnote");

            migrationBuilder.DropTable(
                name: "TimePeriodFootnote");
        }
    }
}
