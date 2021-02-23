using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1710AddMethodologyFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MethodologyFiles");
        }
    }
}
