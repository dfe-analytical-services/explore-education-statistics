using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES2666_MethodologyRenaming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyFiles_Methodologies_MethodologyId",
                table: "MethodologyFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                table: "PublicationMethodologies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PublicationMethodologies",
                table: "PublicationMethodologies");

            migrationBuilder.DropIndex(
                name: "IX_PublicationMethodologies_MethodologyParentId",
                table: "PublicationMethodologies");

            migrationBuilder.DropIndex(
                name: "IX_MethodologyFiles_MethodologyId",
                table: "MethodologyFiles");

            migrationBuilder.DropIndex(
                name: "IX_Methodologies_MethodologyParentId",
                table: "Methodologies");

            migrationBuilder.RenameColumn(
                name: "MethodologyParentId",
                table: "PublicationMethodologies",
                newName: "MethodologyId");

            migrationBuilder.RenameColumn(
                name: "MethodologyId",
                table: "MethodologyFiles",
                newName: "MethodologyVersionId");

            migrationBuilder.RenameColumn(
                name: "MethodologyParentId",
                table: "Methodologies",
                newName: "MethodologyId");

            migrationBuilder.RenameTable(
                name: "Methodologies",
                newName: "MethodologyVersions");

            migrationBuilder.RenameTable(
                name: "MethodologyParents",
                newName: "Methodologies");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PublicationMethodologies",
                table: "PublicationMethodologies",
                columns: new[] { "PublicationId", "MethodologyId" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationMethodologies_MethodologyId",
                table: "PublicationMethodologies",
                column: "MethodologyId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyFiles_MethodologyVersionId",
                table: "MethodologyFiles",
                column: "MethodologyVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyVersions_MethodologyId",
                table: "MethodologyVersions",
                column: "MethodologyId");

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyVersions_Methodologies_MethodologyId",
                table: "MethodologyVersions",
                column: "MethodologyId",
                principalTable: "Methodologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyFiles_MethodologyVersions_MethodologyVersionId",
                table: "MethodologyFiles",
                column: "MethodologyVersionId",
                principalTable: "MethodologyVersions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationMethodologies_Methodologies_MethodologyId",
                table: "PublicationMethodologies",
                column: "MethodologyId",
                principalTable: "Methodologies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicationMethodologies_Methodologies_MethodologyId",
                table: "PublicationMethodologies");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyFiles_MethodologyVersions_MethodologyVersionId",
                table: "MethodologyFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MethodologyVersions_Methodologies_MethodologyId",
                table: "Methodologies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PublicationMethodologies",
                table: "PublicationMethodologies");

            migrationBuilder.DropIndex(
                name: "IX_MethodologyVersions_MethodologyId",
                table: "MethodologyVersions");

            migrationBuilder.DropIndex(
                name: "IX_MethodologyFiles_MethodologyVersionId",
                table: "MethodologyFiles");

            migrationBuilder.DropIndex(
                name: "IX_PublicationMethodologies_MethodologyId",
                table: "PublicationMethodologies");

            migrationBuilder.RenameTable(
                name: "Methodologies",
                newName: "MethodologyParents");

            migrationBuilder.RenameTable(
                name: "MethodologyVersions",
                newName: "Methodologies");

            migrationBuilder.RenameColumn(
                name: "MethodologyId",
                table: "PublicationMethodologies",
                newName:"MethodologyParentId");

            migrationBuilder.RenameColumn(
                name: "MethodologyVersionId",
                table: "MethodologyFiles",
                newName: "MethodologyId");

            migrationBuilder.RenameColumn(
                name: "MethodologyId",
                table: "Methodologies",
                newName: "MethodologyParentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PublicationMethodologies",
                table: "PublicationMethodologies",
                columns: new[] { "PublicationId", "MethodologyParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PublicationMethodologies_MethodologyParentId",
                table: "PublicationMethodologies",
                column: "MethodologyParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyFiles_MethodologyId",
                table: "MethodologyFiles",
                column: "MethodologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Methodologies_MethodologyParentId",
                table: "Methodologies",
                column: "MethodologyParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Methodologies_MethodologyParents_MethodologyParentId",
                table: "Methodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MethodologyFiles_Methodologies_MethodologyId",
                table: "MethodologyFiles",
                column: "MethodologyId",
                principalTable: "Methodologies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationMethodologies_MethodologyParents_MethodologyParentId",
                table: "PublicationMethodologies",
                column: "MethodologyParentId",
                principalTable: "MethodologyParents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
