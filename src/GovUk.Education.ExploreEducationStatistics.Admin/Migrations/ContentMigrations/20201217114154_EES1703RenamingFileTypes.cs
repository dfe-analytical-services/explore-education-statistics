using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1703RenamingFileTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleaseFileType",
                table: "ReleaseFileReferences",
                newName: "Type");

            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFiles_ReleaseFileReferences_ReleaseFileReferenceId",
                table: "ReleaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_ReleaseFiles_ReleaseFileReferenceId",
                table: "ReleaseFiles");

            migrationBuilder.RenameTable(
                name: "ReleaseFileReferences",
                newName: "Files");

            // Rename key
            migrationBuilder.Sql(
                "exec sp_rename 'PK_ReleaseFileReferences', 'PK_Files', 'OBJECT'");

            // Rename foreign keys
            migrationBuilder.Sql(
                "exec sp_rename 'FK_ReleaseFileReferences_ReleaseFileReferences_ReplacedById', 'FK_Files_Files_ReplacedById', 'OBJECT'");
            migrationBuilder.Sql(
                "exec sp_rename 'FK_ReleaseFileReferences_ReleaseFileReferences_ReplacingId', 'FK_Files_Files_ReplacingId', 'OBJECT'");
            migrationBuilder.Sql(
                "exec sp_rename 'FK_ReleaseFileReferences_Releases_ReleaseId', 'FK_Files_Releases_ReleaseId', 'OBJECT'");
            migrationBuilder.Sql(
                "exec sp_rename 'FK_ReleaseFileReferences_ReleaseFileReferences_SourceId', 'FK_Files_Files_SourceId', 'OBJECT'");

            // // Rename foreign key indexes
            migrationBuilder.Sql(
                "exec sp_rename 'Files.IX_ReleaseFileReferences_ReplacedById', 'IX_Files_ReplacedById', 'INDEX'");
            migrationBuilder.Sql(
                "exec sp_rename 'Files.IX_ReleaseFileReferences_ReplacingId', 'IX_Files_ReplacingId', 'INDEX'");
            migrationBuilder.Sql(
                "exec sp_rename 'Files.IX_ReleaseFileReferences_ReleaseId', 'IX_Files_ReleaseId', 'INDEX'");
            migrationBuilder.Sql(
                "exec sp_rename 'Files.IX_ReleaseFileReferences_SourceId', 'IX_Files_SourceId', 'INDEX'");

            migrationBuilder.RenameColumn(
                name: "ReleaseFileReferenceId",
                table: "ReleaseFiles",
                newName: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_FileId",
                table: "ReleaseFiles",
                column: "FileId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFiles_Files_FileId",
                table: "ReleaseFiles",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFiles_Files_FileId",
                table: "ReleaseFiles");

            migrationBuilder.DropIndex(
                name: "IX_ReleaseFiles_FileId",
                table: "ReleaseFiles");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "ReleaseFiles",
                newName: "ReleaseFileReferenceId");

            migrationBuilder.RenameTable(
                name: "Files",
                newName: "ReleaseFileReferences");

            // Rename key
            migrationBuilder.Sql(
                "exec sp_rename 'PK_Files', 'PK_ReleaseFileReferences', 'OBJECT'");

            // Rename foreign keys
            migrationBuilder.Sql(
                "exec sp_rename 'FK_Files_Files_ReplacedById', 'FK_ReleaseFileReferences_ReleaseFileReferences_ReplacedById', 'OBJECT'");
            migrationBuilder.Sql(
                "exec sp_rename 'FK_Files_Files_ReplacingId', 'FK_ReleaseFileReferences_ReleaseFileReferences_ReplacingId', 'OBJECT'");
            migrationBuilder.Sql(
                "exec sp_rename 'FK_Files_Releases_ReleaseId', 'FK_ReleaseFileReferences_Releases_ReleaseId', 'OBJECT'");
            migrationBuilder.Sql(
                "exec sp_rename 'FK_Files_Files_SourceId', 'FK_ReleaseFileReferences_ReleaseFileReferences_SourceId', 'OBJECT'");

            // Rename foreign key indexes
            migrationBuilder.Sql(
                "exec sp_rename 'ReleaseFileReferences.IX_Files_ReplacedById', 'IX_ReleaseFileReferences_ReplacedById', 'INDEX'");
            migrationBuilder.Sql(
                "exec sp_rename 'ReleaseFileReferences.IX_Files_ReplacingId', 'IX_ReleaseFileReferences_ReplacingId', 'INDEX'");
            migrationBuilder.Sql(
                "exec sp_rename 'ReleaseFileReferences.IX_Files_ReleaseId', 'IX_ReleaseFileReferences_ReleaseId', 'INDEX'");
            migrationBuilder.Sql(
                "exec sp_rename 'ReleaseFileReferences.IX_Files_SourceId', 'IX_ReleaseFileReferences_SourceId', 'INDEX'");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_ReleaseFileReferenceId",
                table: "ReleaseFiles",
                column: "ReleaseFileReferenceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFiles_ReleaseFileReferences_ReleaseFileReferenceId",
                table: "ReleaseFiles",
                column: "ReleaseFileReferenceId",
                principalTable: "ReleaseFileReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "ReleaseFileReferences",
                newName: "ReleaseFileType");
        }
    }
}