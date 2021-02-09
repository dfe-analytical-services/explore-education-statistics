using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Migrations.ContentMigrations
{
    public partial class EES1231AddDataImportTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataImports",
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
                    table.PrimaryKey("PK_DataImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataImports_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataImports_Files_MetaFileId",
                        column: x => x.MetaFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataImports_Files_ZipFileId",
                        column: x => x.ZipFileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataImportErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DataImportId = table.Column<Guid>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataImportErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataImportErrors_DataImports_DataImportId",
                        column: x => x.DataImportId,
                        principalTable: "DataImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataImportErrors_DataImportId",
                table: "DataImportErrors",
                column: "DataImportId");

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_FileId",
                table: "DataImports",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_MetaFileId",
                table: "DataImports",
                column: "MetaFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataImports_ZipFileId",
                table: "DataImports",
                column: "ZipFileId",
                unique: true,
                filter: "[ZipFileId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataImportErrors");

            migrationBuilder.DropTable(
                name: "DataImports");
        }
    }
}
