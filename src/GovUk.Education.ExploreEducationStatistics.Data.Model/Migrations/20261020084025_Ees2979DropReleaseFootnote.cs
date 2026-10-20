using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    /// <inheritdoc />
    public partial class Ees2979DropReleaseFootnote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The migrate-footnote-release-versions BAU endpoint has to have forked every shared Footnote and set
            // every ReleaseVersionId. Footnotes which have lost their only ReleaseFootnote since it was last run
            // belong to no ReleaseVersion, so they are deleted along with their links.
            migrationBuilder.Sql(
                @"
                IF EXISTS (SELECT FootnoteId FROM ReleaseFootnote GROUP BY FootnoteId HAVING COUNT(*) > 1)
                    THROW 50000, 'Footnotes are still shared by more than one ReleaseVersion.', 1;

                DELETE FROM Footnote
                WHERE NOT EXISTS (SELECT 1 FROM ReleaseFootnote WHERE ReleaseFootnote.FootnoteId = Footnote.Id);

                IF EXISTS (
                    SELECT 1
                    FROM Footnote
                    JOIN ReleaseFootnote ON ReleaseFootnote.FootnoteId = Footnote.Id
                    WHERE Footnote.ReleaseVersionId IS NULL
                       OR Footnote.ReleaseVersionId <> ReleaseFootnote.ReleaseVersionId
                )
                    THROW 50000, 'Footnotes have a ReleaseVersionId which does not match their ReleaseFootnote.', 1;
                "
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ReleaseVersionId",
                table: "Footnote",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true
            );

            migrationBuilder.DropTable(name: "ReleaseFootnote");

            migrationBuilder.CreateIndex(
                name: "IX_Footnote_ReleaseVersionId",
                table: "Footnote",
                column: "ReleaseVersionId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Footnote_ReleaseVersion_ReleaseVersionId",
                table: "Footnote",
                column: "ReleaseVersionId",
                principalTable: "ReleaseVersion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Footnote_ReleaseVersion_ReleaseVersionId", table: "Footnote");

            migrationBuilder.DropIndex(name: "IX_Footnote_ReleaseVersionId", table: "Footnote");

            migrationBuilder.CreateTable(
                name: "ReleaseFootnote",
                columns: table => new
                {
                    ReleaseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FootnoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFootnote", x => new { x.ReleaseVersionId, x.FootnoteId });
                    table.ForeignKey(
                        name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                        column: x => x.FootnoteId,
                        principalTable: "Footnote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_ReleaseFootnote_ReleaseVersion_ReleaseVersionId",
                        column: x => x.ReleaseVersionId,
                        principalTable: "ReleaseVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFootnote_FootnoteId",
                table: "ReleaseFootnote",
                column: "FootnoteId"
            );

            migrationBuilder.Sql(
                @"
                INSERT INTO ReleaseFootnote (ReleaseVersionId, FootnoteId)
                SELECT ReleaseVersionId, Id
                FROM Footnote;
                "
            );

            migrationBuilder.AlterColumn<Guid>(
                name: "ReleaseVersionId",
                table: "Footnote",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier"
            );
        }
    }
}
