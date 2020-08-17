using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Migrations
{
    public partial class EES1145AddCascadingDeleteToReleaseSubjectsAndFootnotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                table: "ReleaseFootnote");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFootnote_Release_ReleaseId",
                table: "ReleaseFootnote");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseSubject_Release_ReleaseId",
                table: "ReleaseSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseSubject_Subject_SubjectId",
                table: "ReleaseSubject");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                table: "ReleaseFootnote",
                column: "FootnoteId",
                principalTable: "Footnote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFootnote_Release_ReleaseId",
                table: "ReleaseFootnote",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseSubject_Release_ReleaseId",
                table: "ReleaseSubject",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseSubject_Subject_SubjectId",
                table: "ReleaseSubject",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                table: "ReleaseFootnote");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFootnote_Release_ReleaseId",
                table: "ReleaseFootnote");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseSubject_Release_ReleaseId",
                table: "ReleaseSubject");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseSubject_Subject_SubjectId",
                table: "ReleaseSubject");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFootnote_Footnote_FootnoteId",
                table: "ReleaseFootnote",
                column: "FootnoteId",
                principalTable: "Footnote",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFootnote_Release_ReleaseId",
                table: "ReleaseFootnote",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseSubject_Release_ReleaseId",
                table: "ReleaseSubject",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseSubject_Subject_SubjectId",
                table: "ReleaseSubject",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id");
        }
    }
}
