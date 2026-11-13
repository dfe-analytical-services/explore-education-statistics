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
