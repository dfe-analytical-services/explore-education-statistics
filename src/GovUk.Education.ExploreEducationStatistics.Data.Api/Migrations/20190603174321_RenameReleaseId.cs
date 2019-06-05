using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Migrations
{
    public partial class RenameReleaseId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Release_ReleaseIdent",
                table: "Subject");

            migrationBuilder.RenameColumn(
                name: "ReleaseIdent",
                table: "Subject",
                newName: "ReleaseId");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_ReleaseIdent",
                table: "Subject",
                newName: "IX_Subject_ReleaseId");

            migrationBuilder.RenameColumn(
                name: "Ident",
                table: "Release",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Release_ReleaseId",
                table: "Subject",
                column: "ReleaseId",
                principalTable: "Release",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subject_Release_ReleaseId",
                table: "Subject");

            migrationBuilder.RenameColumn(
                name: "ReleaseId",
                table: "Subject",
                newName: "ReleaseIdent");

            migrationBuilder.RenameIndex(
                name: "IX_Subject_ReleaseId",
                table: "Subject",
                newName: "IX_Subject_ReleaseIdent");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Release",
                newName: "Ident");

            migrationBuilder.AddForeignKey(
                name: "FK_Subject_Release_ReleaseIdent",
                table: "Subject",
                column: "ReleaseIdent",
                principalTable: "Release",
                principalColumn: "Ident",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
