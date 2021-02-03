using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1231AddDataImportTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Imports",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    StagePercentageComplete = table.Column<int>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    MetaFileId = table.Column<Guid>(nullable: false),
                    ZipFileId = table.Column<Guid>(nullable: true),
                    Rows = table.Column<int>(nullable: false),
                    NumBatches = table.Column<int>(nullable: false),
                    RowsPerBatch = table.Column<int>(nullable: false),
                    TotalRows = table.Column<int>(nullable: false),
                    Migrated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imports_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Imports_Files_MetaFileId",
                        column: x => x.MetaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Imports_Files_ZipFileId",
                        column: x => x.ZipFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImportErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ImportId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportErrors_Imports_ImportId",
                        column: x => x.ImportId,
                        principalTable: "Imports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportErrors_ImportId",
                table: "ImportErrors",
                column: "ImportId");

            migrationBuilder.CreateIndex(
                name: "IX_Imports_FileId",
                table: "Imports",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imports_MetaFileId",
                table: "Imports",
                column: "MetaFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imports_ZipFileId",
                table: "Imports",
                column: "ZipFileId",
                unique: true,
                filter: "[ZipFileId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportErrors");

            migrationBuilder.DropTable(
                name: "Imports");
        }
    }
}
