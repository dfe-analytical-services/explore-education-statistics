using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class AddReleaseFileLinkTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReleaseFileReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: true),
                    Filename = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFileReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseFileReferences_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReleaseFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ReleaseId = table.Column<Guid>(nullable: false),
                    ReleaseFileReferenceId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseFiles_ReleaseFileReferences_ReleaseFileReferenceId",
                        column: x => x.ReleaseFileReferenceId,
                        principalTable: "ReleaseFileReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseFiles_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFileReferences_ReleaseId",
                table: "ReleaseFileReferences",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_ReleaseFileReferenceId",
                table: "ReleaseFiles",
                column: "ReleaseFileReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseFiles_ReleaseId",
                table: "ReleaseFiles",
                column: "ReleaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReleaseFiles");

            migrationBuilder.DropTable(
                name: "ReleaseFileReferences");
        }
    }
}
