using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1710AddMethodologyFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFiles_Files_FileId",
                table: "ReleaseFiles");

            migrationBuilder.CreateTable(
                name: "MethodologyFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MethodologyId = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodologyFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MethodologyFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MethodologyFiles_Methodologies_MethodologyId",
                        column: x => x.MethodologyId,
                        principalTable: "Methodologies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyFiles_FileId",
                table: "MethodologyFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodologyFiles_MethodologyId",
                table: "MethodologyFiles",
                column: "MethodologyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFiles_Files_FileId",
                table: "ReleaseFiles",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReleaseFiles_Files_FileId",
                table: "ReleaseFiles");

            migrationBuilder.DropTable(
                name: "MethodologyFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseFiles_Files_FileId",
                table: "ReleaseFiles",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
