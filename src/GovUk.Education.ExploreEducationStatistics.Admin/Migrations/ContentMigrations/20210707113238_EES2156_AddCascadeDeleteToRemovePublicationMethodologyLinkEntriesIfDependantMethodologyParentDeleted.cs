using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2156_AddCascadeDeleteToRemovePublicationMethodologyLinkEntriesIfDependantMethodologyParentDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                table: "PublicationMethodologies");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                table: "PublicationMethodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                table: "PublicationMethodologies");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                table: "PublicationMethodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id");
        }
    }
}
