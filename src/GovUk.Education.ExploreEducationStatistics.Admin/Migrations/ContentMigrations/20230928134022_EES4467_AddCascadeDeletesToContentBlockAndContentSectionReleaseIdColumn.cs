using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES4467_AddCascadeDeletesToContentBlockAndContentSectionReleaseIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove the cascade delete that causes Featured Tables to be deleted when their owning Releases
            // are deleted. Instead, we are going to retain the cascade delete via another FeaturedTables
            // foreign key "FK_FeaturedTables_ContentBlock_DataBlockId", which will delete the associated
            // FeaturedTables row when its referenced ContentBlock is deleted. The reason we can't retain this cascade
            // delete anymore is that we will be adding a new cascade delete to the ContentBlock table that will delete
            // all ContentBlocks related to a given Release, and if this FeaturedTables cascade delete had remained, it
            // would generate multiple cascade paths for FeaturedTable rows.
            migrationBuilder.DropForeignKey(
                name: "FK_FeaturedTables_Releases_ReleaseId",
                table: "FeaturedTables");
            
            migrationBuilder.AddForeignKey(
                name: "FK_FeaturedTables_Releases_ReleaseId",
                table: "FeaturedTables",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            // Amend the ContentBlock foreign key cascade now that the cyclical cascade delete from the
            // ReleaseContentBlocks table is no longer present.
            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlock_Releases_ReleaseId",
                table: "ContentBlock");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlock_Releases_ReleaseId",
                table: "ContentBlock",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            
            // Amend the ContentSections foreign key cascade now that the cyclical cascade delete from the
            // ReleaseContentSections table is no longer present.
            migrationBuilder.DropForeignKey(
                name: "FK_ContentSections_Releases_ReleaseId",
                table: "ContentSections");
            
            migrationBuilder.AddForeignKey(
                name: "FK_ContentSections_Releases_ReleaseId",
                table: "ContentSections",
                column: "ReleaseId",
                principalTable: "Releases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
