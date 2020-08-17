using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1145AddReleaseFileReferenceCascadingDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFileReferences_Releases_ReleaseId",
                table: "ReleaseFileReferences");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFileReferences_Releases_ReleaseId",
                table: "ReleaseFileReferences",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFileReferences_Releases_ReleaseId",
                table: "ReleaseFileReferences");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFileReferences_Releases_ReleaseId",
                table: "ReleaseFileReferences",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id");
        }
    }
}
