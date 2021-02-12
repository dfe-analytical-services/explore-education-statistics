using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1702RemoveReleaseFromFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Releases_ReleaseId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ReleaseId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "ReleaseId",
                table: "Files",
                newName: "BlobPath");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BlobPath",
                table: "Files",
                newName: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_ReleaseId",
                table: "Files",
                column: "ReleaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Releases_ReleaseId",
                table: "Files",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
